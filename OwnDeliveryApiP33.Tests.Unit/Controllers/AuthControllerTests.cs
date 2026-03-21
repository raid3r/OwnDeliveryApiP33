using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Application.Validators;
using OwnDeliveryApiP33.Controllers;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Tests.Unit.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AuthController _sut;
    private readonly IAuthService _authService;

    private static IConfiguration BuildJwtConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]              = "UnitTestSuperSecretKey_AtLeast32Chars!",
                ["Jwt:Issuer"]           = "TestIssuer",
                ["Jwt:Audience"]         = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
            .Options;
        _context = new ApplicationDbContext(options);

        var config = BuildJwtConfig();
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Courier>();
        var tokenService = new TokenService(config);
        
        _authService = new AuthService(
            _context,
            tokenService,
            new RegisterCourierRequestValidator(),
            new LoginCourierRequestValidator(),
            passwordHasher);

        _sut = new AuthController(_authService);
    }

    // ── Register ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidRequest_Returns201AndToken()
    {
        var request = new RegisterCourierRequest("Jane", "Smith", "jane@example.com", "Pass123", "+380501112233");

        var result = await _sut.Register(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = created.Value.Should().BeOfType<AuthResponse>().Subject;

        response.Email.Should().Be("jane@example.com");
        response.FirstName.Should().Be("Jane");
        response.LastName.Should().Be("Smith");
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_SavesToDatabase()
    {
        var request = new RegisterCourierRequest("Ivan", "Petrov", "ivan@example.com", "Pass123", "+380509876543");

        await _sut.Register(request, CancellationToken.None);

        var saved = await _context.Couriers.SingleOrDefaultAsync(c => c.Email == "ivan@example.com");
        saved.Should().NotBeNull();
        saved!.FirstName.Should().Be("Ivan");
        saved.IsActive.Should().BeTrue();
        saved.PasswordHash.Should().NotBe(request.Password); // must be hashed
    }

    [Fact]
    public async Task Register_NormalizesEmailToLowercase()
    {
        var request = new RegisterCourierRequest("John", "Doe", "John.Doe@EXAMPLE.COM", "Pass123", "+1234567890");

        await _sut.Register(request, CancellationToken.None);

        var saved = await _context.Couriers.SingleAsync();
        saved.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = "dup@example.com";
        var first = new RegisterCourierRequest("A", "B", email, "Pass123", "+1");
        var second = new RegisterCourierRequest("C", "D", email, "Pass456", "+2");

        await _sut.Register(first, CancellationToken.None);
        var result = await _sut.Register(second, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Register_DuplicateEmailCaseInsensitive_Returns409()
    {
        var first  = new RegisterCourierRequest("A", "B", "user@example.com",  "Pass123", "+1");
        var second = new RegisterCourierRequest("C", "D", "USER@EXAMPLE.COM",  "Pass456", "+2");

        await _sut.Register(first, CancellationToken.None);
        var result = await _sut.Register(second, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Register_InvalidRequest_Returns400()
    {
        var request = new RegisterCourierRequest("", "", "not-an-email", "123", "");

        var result = await _sut.Register(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Login ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        await SeedCourierAsync("login@example.com", "Secret1");

        var result = await _sut.Login(new LoginCourierRequest("login@example.com", "Secret1"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<AuthResponse>().Subject;

        response.Email.Should().Be("login@example.com");
        response.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await SeedCourierAsync("wrong@example.com", "CorrectPass1");

        var result = await _sut.Login(new LoginCourierRequest("wrong@example.com", "WrongPass"), CancellationToken.None);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var result = await _sut.Login(new LoginCourierRequest("ghost@example.com", "Pass123"), CancellationToken.None);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_InvalidRequest_Returns400()
    {
        var result = await _sut.Login(new LoginCourierRequest("bad-email", ""), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_CaseInsensitiveEmail_Returns200()
    {
        await SeedCourierAsync("case@example.com", "Pass123");

        var result = await _sut.Login(new LoginCourierRequest("CASE@EXAMPLE.COM", "Pass123"), CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GenerateToken ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_TokenContainsCorrectClaims()
    {
        var request = new RegisterCourierRequest("Alice", "Wonder", "alice@example.com", "Pass123", "+380");

        var actionResult = await _sut.Register(request, CancellationToken.None);

        var response = ((CreatedAtActionResult)actionResult).Value as AuthResponse;
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(response!.Token);

        jwtToken.Subject.Should().Be(response.CourierId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "alice@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == "given_name" && c.Value == "Alice");
        jwtToken.Claims.Should().Contain(c => c.Type == "family_name" && c.Value == "Wonder");
        jwtToken.Issuer.Should().Be("TestIssuer");
    }

    [Fact]
    public async Task Register_TokenExpiresInAbout60Minutes()
    {
        var request = new RegisterCourierRequest("Bob", "Builder", "bob@example.com", "Pass123", "+380");

        var actionResult = await _sut.Register(request, CancellationToken.None);

        var response = ((CreatedAtActionResult)actionResult).Value as AuthResponse;
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(response!.Token);

        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(30));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private async Task SeedCourierAsync(string email, string password)
    {
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Courier>();
        var courier = new Courier
        {
            Id          = Guid.NewGuid(),
            FirstName   = "Test",
            LastName    = "User",
            Email       = email.ToLower(),
            PhoneNumber = "+380501234567",
            CreatedAt   = DateTime.UtcNow,
            IsActive    = true
        };
        courier.PasswordHash = passwordHasher.HashPassword(courier, password);
        _context.Couriers.Add(courier);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();
}
