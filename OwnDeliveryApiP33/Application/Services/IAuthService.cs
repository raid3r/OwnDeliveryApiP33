using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Services;

public interface IAuthService
{
    /// <summary>Register a new courier</summary>
    Task<AuthResponse> RegisterAsync(RegisterCourierRequest request, CancellationToken ct = default);

    /// <summary>Login as a courier and receive a JWT token</summary>
    Task<AuthResponse> LoginAsync(LoginCourierRequest request, CancellationToken ct = default);
}
