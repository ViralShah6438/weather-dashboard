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

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
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
            return NotFound(new ProblemDetails
            {
                Title = "City not found",
                Detail = "No weather data was found for the specified city.",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (WeatherProviderException ex)
        {
            return Problem(
                title: "Weather provider error",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}

