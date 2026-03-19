using WeatherDashboard.Api.Contracts;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace WeatherDashboard.Api.Controllers;

[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly IDefaultLocationService _defaultLocationService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(IDefaultLocationService defaultLocationService, ILogger<SettingsController> logger)
    {
        _defaultLocationService = defaultLocationService;
        _logger = logger;
    }

    [HttpGet("default-location")]
    [ProducesResponseType(typeof(DefaultLocationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDefaultLocation(CancellationToken cancellationToken)
    {
        var city = await _defaultLocationService.GetAsync(cancellationToken);
        return Ok(new DefaultLocationResponse { City = city });
    }

    [HttpPut("default-location")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultLocation([FromBody] SetDefaultLocationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.City))
        {
            _logger.LogWarning("Set default location rejected: city parameter is empty");
            return ValidationProblem(new ValidationProblemDetails
            {
                Title = "Validation error",
                Detail = "City is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            await _defaultLocationService.SetAsync(request.City, cancellationToken);
            _logger.LogInformation("Default location updated to {City}", request.City);
            return NoContent();
        }
        catch (CityNotFoundException)
        {
            _logger.LogWarning("Set default location failed: city not found {City}", request.City);
            return NotFound(new ProblemDetails
            {
                Title = "City not found",
                Detail = "The specified city could not be found. Please enter a valid city name.",
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}

