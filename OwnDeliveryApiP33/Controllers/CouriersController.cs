using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Services;

namespace OwnDeliveryApiP33.Controllers;

[ApiController]
[Route("api/v1/couriers")]
[Produces("application/json")]
[Authorize]
public class CouriersController : ControllerBase
{
    private readonly ICourierService _courierService;

    public CouriersController(ICourierService courierService)
    {
        _courierService = courierService;
    }

    /// <summary>Get the profile of the currently authenticated courier</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CourierProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(sub, out var courierId))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        try
        {
            var profile = await _courierService.GetProfileAsync(courierId, ct);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
