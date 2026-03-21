using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace OwnDeliveryApiP33.Tests.Unit.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly IConfiguration _configuration;

    private static IConfiguration BuildJwtConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyForJWT_AtLeast32Characters!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

    public TokenServiceTests()
    {
        _configuration = BuildJwtConfig();
        _sut = new TokenService(_configuration);
    }

    // ?? GenerateToken - Basic Functionality ??????????????????????????????????

    [Fact]
    public void GenerateToken_WithValidCourier_ReturnsTokenAndExpiresAt()
    {
        var courier = CreateTestCourier();

        var (token, expiresAt) = _sut.GenerateToken(courier);

        token.Should().NotBeNullOrWhiteSpace();
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtToken()
    {
        var courier = CreateTestCourier();

        var (token, _) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var canRead = handler.CanReadToken(token);
        canRead.Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_TokenCanBeParsed()
    {
        var courier = CreateTestCourier();

        var (token, _) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Should().NotBeNull();
    }

    // ?? GenerateToken - Claims ???????????????????????????????????????????????

    [Fact]
    public void GenerateToken_ContainsCourierIdInSubjectClaim()
    {
        var courier = CreateTestCourier();

        var (token, _) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Subject.Should().Be(courier.Id.ToString());
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var courier = CreateTestCourier("test@example.com");

        var (token, _) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
    }

    [Fact]
    public void GenerateToken_ContainsFirstNameClaim()
    {
        var courier = CreateTestCourier(firstName: "John");

        var (token, _) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "given_name" && c.Value == "John");
    }

    [Fact]
    public void GenerateToken_ContainsLastNameClaim()
    {
        var courier = CreateTestCourier(lastName: "Doe");

        var (token, _) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "family_name" && c.Value == "Doe");
    }

    [Fact]
    public void GenerateToken_ContainsAllRequiredClaims()
    {
        var courier = CreateTestCourier();

        var (token, _) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Should contain: sub, email, given_name, family_name
        jwtToken.Claims.Count().Should().BeGreaterThanOrEqualTo(4);
        jwtToken.Claims.Should().Contain(c => c.Type == "sub");
        jwtToken.Claims.Should().Contain(c => c.Type == "email");
        jwtToken.Claims.Should().Contain(c => c.Type == "given_name");
        jwtToken.Claims.Should().Contain(c => c.Type == "family_name");
    }

    // ?? GenerateToken - Expiration ???????????????????????????????????????????

    [Fact]
    public void GenerateToken_ExpiresAtIsSetCorrectly()
    {
        var courier = CreateTestCourier();
        var beforeCall = DateTime.UtcNow;

        var (_, expiresAt) = _sut.GenerateToken(courier);

        var afterCall = DateTime.UtcNow;
        
        // ExpiresAt should be approximately 60 minutes from now
        expiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
        expiresAt.Should().BeAfter(beforeCall);
    }

    [Fact]
    public void GenerateToken_TokenExpirationMatchesReturnedExpiresAt()
    {
        var courier = CreateTestCourier();

        var (token, expiresAt) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
    }

    // ?? GenerateToken - Issuer and Audience ??????????????????????????????????

    [Fact]
    public void GenerateToken_ContainsCorrectIssuer()
    {
        var courier = CreateTestCourier();

        var (token, _) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("TestIssuer");
    }

    [Fact]
    public void GenerateToken_ContainsCorrectAudience()
    {
        var courier = CreateTestCourier();

        var (token, _) = _sut.GenerateToken(courier);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    // ?? Configuration Variations ?????????????????????????????????????????????

    [Theory]
    [InlineData("30")]
    [InlineData("120")]
    [InlineData("1440")] // 1 day
    public void GenerateToken_RespectsDifferentExpirationMinutes(string expirationMinutes)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyForJWT_AtLeast32Characters!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = expirationMinutes
            })
            .Build();

        var service = new TokenService(config);
        var courier = CreateTestCourier();

        var (_, expiresAt) = service.GenerateToken(courier);

        var expectedExpiration = double.Parse(expirationMinutes);
        expiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(expectedExpiration),
            TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_UsesDefaultExpirationWhenConfigMissing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyForJWT_AtLeast32Characters!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                // Note: No ExpiresInMinutes configured
            })
            .Build();

        var service = new TokenService(config);
        var courier = CreateTestCourier();

        var (_, expiresAt) = service.GenerateToken(courier);

        // Should default to 60 minutes
        expiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    // ?? GenerateToken - Multiple Calls ???????????????????????????????????????

    [Fact]
    public void GenerateToken_MultipleCallsWithDifferentExpiry_ProducesDifferentExpiresAt()
    {
        var courier = CreateTestCourier();

        var (token1, expiresAt1) = _sut.GenerateToken(courier);
        
        // Small delay to ensure different expiration time
        System.Threading.Thread.Sleep(100);
        
        var (token2, expiresAt2) = _sut.GenerateToken(courier);

        // Tokens may be same if generated within same second, but expiry times should be different
        expiresAt1.Should().NotBe(expiresAt2);
    }

    [Fact]
    public void GenerateToken_DifferentCouriersDifferentSubjects()
    {
        var courier1 = CreateTestCourier();
        var courier2 = CreateTestCourier();

        var (token1, _) = _sut.GenerateToken(courier1);
        var (token2, _) = _sut.GenerateToken(courier2);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        jwtToken1.Subject.Should().NotBe(jwtToken2.Subject);
    }

    // ?? Helpers ??????????????????????????????????????????????????????????????

    private static Courier CreateTestCourier(
        string? email = null,
        string? firstName = null,
        string? lastName = null)
    {
        return new Courier
        {
            Id = Guid.NewGuid(),
            FirstName = firstName ?? "John",
            LastName = lastName ?? "Doe",
            Email = email ?? "john@example.com",
            PhoneNumber = "+380501234567",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}
