using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Application.Services;

public interface ITokenService
{
    /// <summary>Generate JWT token for a courier</summary>
    (string Token, DateTime ExpiresAt) GenerateToken(Courier courier);
}
