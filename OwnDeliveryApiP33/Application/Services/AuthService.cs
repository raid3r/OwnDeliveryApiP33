using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Application.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegisterCourierRequest> _registerValidator;
    private readonly IValidator<LoginCourierRequest> _loginValidator;
    private readonly PasswordHasher<Courier> _passwordHasher;

    public AuthService(
        ApplicationDbContext context,
        ITokenService tokenService,
        IValidator<RegisterCourierRequest> registerValidator,
        IValidator<LoginCourierRequest> loginValidator,
        PasswordHasher<Courier> passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterCourierRequest request, CancellationToken ct = default)
    {
        var validation = await _registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var exists = await _context.Couriers.AnyAsync(c => c.Email == request.Email.ToLower(), ct);
        if (exists)
        {
            throw new InvalidOperationException("A courier with this email already exists.");
        }

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

        var (token, expiresAt) = _tokenService.GenerateToken(courier);
        return new AuthResponse(
            courier.Id,
            courier.Email,
            courier.FirstName,
            courier.LastName,
            token,
            expiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginCourierRequest request, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.Email == request.Email.ToLower(), ct);

        if (courier is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(courier, courier.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var (token, expiresAt) = _tokenService.GenerateToken(courier);
        return new AuthResponse(
            courier.Id,
            courier.Email,
            courier.FirstName,
            courier.LastName,
            token,
            expiresAt);
    }
}
