using WeatherDashboard.Api.Contracts;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace WeatherDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [EnableRateLimiting("Concurrency")]
    public async Task<IActionResult> GetByCity([FromQuery] string city, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            _logger.LogWarning("Weather request rejected: city parameter is empty");
            return ValidationProblem(new ValidationProblemDetails
            {
                Title = "Validation error",
                Detail = "City is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var snapshot = await _weatherService.GetByCityAsync(city, cancellationToken);
            return Ok(new WeatherResponse
            {
                City = snapshot.City,
                TemperatureC = snapshot.TemperatureC,
                Humidity = snapshot.Humidity,
                WindSpeedKph = snapshot.WindSpeedKph,
                IconCode = snapshot.IconCode,
                Description = snapshot.Description
            });
        }
        catch (CityNotFoundException)
        {
            _logger.LogWarning("City not found: {City}", city);
            return NotFound(new ProblemDetails
            {
                Title = "City not found",
                Detail = "No weather data was found for the specified city.",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (WeatherProviderException ex)
        {
            _logger.LogError(ex, "Weather provider error for city {City}", city);
            return Problem(
                title: "Weather provider error",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}

