using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Application.Validators;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Tests.Unit.Services;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly PasswordHasher<Courier> _passwordHasher;
    private readonly AuthService _sut;

    private static readonly (string Token, DateTime ExpiresAt) FakeTokenResult =
        ("fake.jwt.token", DateTime.UtcNow.AddHours(1));

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _tokenService = Substitute.For<ITokenService>();
        _tokenService.GenerateToken(Arg.Any<Courier>()).Returns(FakeTokenResult);

        _passwordHasher = new PasswordHasher<Courier>();

        _sut = new AuthService(
            _context,
            _tokenService,
            new RegisterCourierRequestValidator(),
            new LoginCourierRequestValidator(),
            _passwordHasher);
    }

    // ── RegisterAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponse()
    {
        var request = new RegisterCourierRequest("Ivan", "Petrenko", "ivan@example.com", "Pass123", "+380501234567");

        var result = await _sut.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Email.Should().Be("ivan@example.com");
        result.FirstName.Should().Be("Ivan");
        result.LastName.Should().Be("Petrenko");
        result.Token.Should().Be(FakeTokenResult.Token);
        result.ExpiresAt.Should().Be(FakeTokenResult.ExpiresAt);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_SavesCourierToDatabase()
    {
        var request = new RegisterCourierRequest("Olga", "Koval", "olga@example.com", "Pass123", "+380671234567");

        await _sut.RegisterAsync(request);

        var saved = await _context.Couriers.SingleOrDefaultAsync(c => c.Email == "olga@example.com");
        saved.Should().NotBeNull();
        saved!.FirstName.Should().Be("Olga");
        saved.LastName.Should().Be("Koval");
        saved.PhoneNumber.Should().Be("+380671234567");
        saved.IsActive.Should().BeTrue();
        saved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmailToLowercase()
    {
        var request = new RegisterCourierRequest("John", "Doe", "John.Doe@EXAMPLE.COM", "Pass123", "+1234567890");

        await _sut.RegisterAsync(request);

        var saved = await _context.Couriers.SingleAsync();
        saved.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        var request = new RegisterCourierRequest("Alice", "Wonder", "alice@example.com", "MySecret99", "+380");

        await _sut.RegisterAsync(request);

        var saved = await _context.Couriers.SingleAsync();
        saved.PasswordHash.Should().NotBe(request.Password);
        saved.PasswordHash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegisterAsync_ReturnedCourierIdMatchesDatabaseEntry()
    {
        var request = new RegisterCourierRequest("Mark", "Twain", "mark@example.com", "Pass123", "+380");

        var result = await _sut.RegisterAsync(request);

        var saved = await _context.Couriers.SingleAsync();
        result.CourierId.Should().Be(saved.Id);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var request = new RegisterCourierRequest("A", "B", "dup@example.com", "Pass123", "+1");
        await _sut.RegisterAsync(request);

        var act = () => _sut.RegisterAsync(
            new RegisterCourierRequest("C", "D", "dup@example.com", "Pass456", "+2"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmailCaseInsensitive_ThrowsInvalidOperationException()
    {
        await _sut.RegisterAsync(new RegisterCourierRequest("A", "B", "user@example.com", "Pass123", "+1"));

        var act = () => _sut.RegisterAsync(
            new RegisterCourierRequest("C", "D", "USER@EXAMPLE.COM", "Pass456", "+2"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RegisterAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new RegisterCourierRequest("", "", "not-an-email", "123", "");

        var act = () => _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RegisterAsync_CallsTokenService()
    {
        var request = new RegisterCourierRequest("Kate", "Green", "kate@example.com", "Pass123", "+380");

        await _sut.RegisterAsync(request);

        _tokenService.Received(1).GenerateToken(Arg.Any<Courier>());
    }

    // ── LoginAsync ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        await SeedCourierAsync("login@example.com", "Secret1");

        var result = await _sut.LoginAsync(new LoginCourierRequest("login@example.com", "Secret1"));

        result.Should().NotBeNull();
        result.Email.Should().Be("login@example.com");
        result.Token.Should().Be(FakeTokenResult.Token);
        result.ExpiresAt.Should().Be(FakeTokenResult.ExpiresAt);
    }

    [Fact]
    public async Task LoginAsync_CaseInsensitiveEmailLookup_ReturnsAuthResponse()
    {
        await SeedCourierAsync("case@example.com", "Pass123");

        var result = await _sut.LoginAsync(new LoginCourierRequest("CASE@EXAMPLE.COM", "Pass123"));

        result.Should().NotBeNull();
        result.Email.Should().Be("case@example.com");
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorizedAccessException()
    {
        var act = () => _sut.LoginAsync(new LoginCourierRequest("ghost@example.com", "Pass123"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        await SeedCourierAsync("wrong@example.com", "CorrectPass1");

        var act = () => _sut.LoginAsync(new LoginCourierRequest("wrong@example.com", "WrongPass"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_InvalidRequest_ThrowsValidationException()
    {
        var act = () => _sut.LoginAsync(new LoginCourierRequest("bad-email", ""));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_CallsTokenService()
    {
        await SeedCourierAsync("token@example.com", "Pass123");

        await _sut.LoginAsync(new LoginCourierRequest("token@example.com", "Pass123"));

        _tokenService.Received(1).GenerateToken(Arg.Any<Courier>());
    }

    [Fact]
    public async Task LoginAsync_ReturnedCourierIdMatchesDatabaseEntry()
    {
        var seededId = await SeedCourierAsync("id@example.com", "Pass123");

        var result = await _sut.LoginAsync(new LoginCourierRequest("id@example.com", "Pass123"));

        result.CourierId.Should().Be(seededId);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private async Task<Guid> SeedCourierAsync(string email, string password)
    {
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
        courier.PasswordHash = _passwordHasher.HashPassword(courier, password);
        _context.Couriers.Add(courier);
        await _context.SaveChangesAsync();
        return courier.Id;
    }

    public void Dispose() => _context.Dispose();
}
