using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IValidator<RegisterCourierRequest> _registerValidator;
    private readonly IValidator<LoginCourierRequest> _loginValidator;
    private readonly PasswordHasher<Courier> _passwordHasher;

    public AuthController(
        ApplicationDbContext context,
        IConfiguration configuration,
        IValidator<RegisterCourierRequest> registerValidator,
        IValidator<LoginCourierRequest> loginValidator,
        PasswordHasher<Courier> passwordHasher)
    {
        _context = context;
        _configuration = configuration;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _passwordHasher = passwordHasher;
    }

    /// <summary>Register a new courier</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCourierRequest request, CancellationToken ct)
    {
        var validation = await _registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var exists = await _context.Couriers.AnyAsync(c => c.Email == request.Email.ToLower(), ct);
        if (exists)
            return Conflict(new { message = "A courier with this email already exists." });

        var courier = new Courier
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLower(),
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        courier.PasswordHash = _passwordHasher.HashPassword(courier, request.Password);

        _context.Couriers.Add(courier);
        await _context.SaveChangesAsync(ct);

        var response = GenerateToken(courier);
        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>Login as a courier and receive a JWT token</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCourierRequest request, CancellationToken ct)
    {
        var validation = await _loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.Email == request.Email.ToLower(), ct);

        if (courier is null)
            return Unauthorized(new { message = "Invalid email or password." });

        var result = _passwordHasher.VerifyHashedPassword(courier, courier.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        var response = GenerateToken(courier);
        return Ok(response);
    }

    private AuthResponse GenerateToken(Courier courier)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, courier.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, courier.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, courier.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, courier.LastName),
            new Claim("courier_id", courier.Id.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponse(
            courier.Id,
            courier.Email,
            courier.FirstName,
            courier.LastName,
            new JwtSecurityTokenHandler().WriteToken(token),
            expires);
    }
}
