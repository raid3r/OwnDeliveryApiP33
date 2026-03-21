using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Application.Services;

public class CourierService : ICourierService
{
    private readonly ApplicationDbContext _context;

    public CourierService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourierProfileResponse> GetProfileAsync(Guid courierId, CancellationToken ct = default)
    {
        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.Id == courierId, ct);

        if (courier is null)
        {
            throw new KeyNotFoundException("Courier not found.");
        }

        return new CourierProfileResponse(
            courier.Id,
            courier.Email,
            courier.FirstName,
            courier.LastName,
            courier.PhoneNumber,
            courier.CreatedAt,
            courier.IsActive);
    }
}
