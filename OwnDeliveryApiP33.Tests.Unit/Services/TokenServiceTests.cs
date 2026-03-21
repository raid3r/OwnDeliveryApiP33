using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Tests.Unit.Services;

public class TokenServiceTests
{
    private static IConfiguration BuildConfig(
        string key = "UnitTestSuperSecretKey_AtLeast32Chars!",
        string issuer = "TestIssuer",
        string audience = "TestAudience",
        string expiresInMinutes = "60") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]              = key,
                ["Jwt:Issuer"]           = issuer,
                ["Jwt:Audience"]         = audience,
                ["Jwt:ExpiresInMinutes"] = expiresInMinutes
            })
            .Build();

    private static Courier BuildCourier(
        string firstName = "Jane",
        string lastName  = "Smith",
        string email     = "jane@example.com") =>
        new()
        {
            Id           = Guid.NewGuid(),
            FirstName    = firstName,
            LastName     = lastName,
            Email        = email,
            PasswordHash = "hashed",
            PhoneNumber  = "+380501234567",
            CreatedAt    = DateTime.UtcNow,
            IsActive     = true
        };

    // ── Token structure ──────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_ReturnsNonEmptyTokenString()
    {
        var sut = new TokenService(BuildConfig());

        var (token, _) = sut.GenerateToken(BuildCourier());

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtToken()
    {
        var sut = new TokenService(BuildConfig());

        var (token, _) = sut.GenerateToken(BuildCourier());

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_TokenContainsCourierIdAsSub()
    {
        var courier = BuildCourier();
        var sut = new TokenService(BuildConfig());

        var (token, _) = sut.GenerateToken(courier);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Subject.Should().Be(courier.Id.ToString());
    }

    [Fact]
    public void GenerateToken_TokenContainsEmailClaim()
    {
        var courier = BuildCourier(email: "test@mail.com");
        var sut = new TokenService(BuildConfig());

        var (token, _) = sut.GenerateToken(courier);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == courier.Email);
    }

    [Fact]
    public void GenerateToken_TokenContainsGivenNameClaim()
    {
        var courier = BuildCourier(firstName: "Olena");
        var sut = new TokenService(BuildConfig());

        var (token, _) = sut.GenerateToken(courier);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == "given_name" && c.Value == "Olena");
    }

    [Fact]
    public void GenerateToken_TokenContainsFamilyNameClaim()
    {
        var courier = BuildCourier(lastName: "Kovalenko");
        var sut = new TokenService(BuildConfig());

        var (token, _) = sut.GenerateToken(courier);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == "family_name" && c.Value == "Kovalenko");
    }

    // ── Issuer / Audience ────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_UsesConfiguredIssuer()
    {
        var sut = new TokenService(BuildConfig(issuer: "MyIssuer"));

        var (token, _) = sut.GenerateToken(BuildCourier());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("MyIssuer");
    }

    [Fact]
    public void GenerateToken_UsesConfiguredAudience()
    {
        var sut = new TokenService(BuildConfig(audience: "MyAudience"));

        var (token, _) = sut.GenerateToken(BuildCourier());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Audiences.Should().Contain("MyAudience");
    }

    // ── Expiry ───────────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_ExpiresAtIsReturnedCorrectly()
    {
        var sut = new TokenService(BuildConfig(expiresInMinutes: "60"));
        var before = DateTime.UtcNow;

        var (_, expiresAt) = sut.GenerateToken(BuildCourier());

        expiresAt.Should().BeCloseTo(before.AddMinutes(60), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GenerateToken_TokenValidToMatchesReturnedExpiresAt()
    {
        var sut = new TokenService(BuildConfig(expiresInMinutes: "30"));

        var (token, expiresAt) = sut.GenerateToken(BuildCourier());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.ValidTo.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_RespectsCustomExpiresInMinutes()
    {
        var sut = new TokenService(BuildConfig(expiresInMinutes: "120"));
        var before = DateTime.UtcNow;

        var (token, _) = sut.GenerateToken(BuildCourier());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.ValidTo.Should().BeCloseTo(before.AddMinutes(120), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GenerateToken_DefaultsTo60MinutesWhenExpiresInMinutesInvalid()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]              = "UnitTestSuperSecretKey_AtLeast32Chars!",
                ["Jwt:Issuer"]           = "TestIssuer",
                ["Jwt:Audience"]         = "TestAudience",
                ["Jwt:ExpiresInMinutes"] = "not-a-number"
            })
            .Build();
        var sut = new TokenService(config);
        var before = DateTime.UtcNow;

        var (token, _) = sut.GenerateToken(BuildCourier());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.ValidTo.Should().BeCloseTo(before.AddMinutes(60), TimeSpan.FromSeconds(10));
    }

    // ── Algorithm ────────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_UsesHmacSha256Algorithm()
    {
        var sut = new TokenService(BuildConfig());

        var (token, _) = sut.GenerateToken(BuildCourier());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Header.Alg.Should().Be("HS256");
    }

    // ── Uniqueness ───────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateToken_DifferentCouriersProduceDifferentTokens()
    {
        var sut = new TokenService(BuildConfig());
        var courier1 = BuildCourier(email: "one@example.com");
        var courier2 = BuildCourier(email: "two@example.com");

        var (token1, _) = sut.GenerateToken(courier1);
        var (token2, _) = sut.GenerateToken(courier2);

        token1.Should().NotBe(token2);
    }
}
