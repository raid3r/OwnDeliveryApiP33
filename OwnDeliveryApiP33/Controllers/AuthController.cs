using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Application.Services;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Register a new courier</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCourierRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, ct);
            return CreatedAtAction(nameof(Register), response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Login as a courier and receive a JWT token</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCourierRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.LoginAsync(request, ct);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
