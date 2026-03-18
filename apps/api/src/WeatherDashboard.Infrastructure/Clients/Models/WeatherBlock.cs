namespace WeatherDashboard.Infrastructure.Clients.Models;

/// <summary>
/// Represents a weather condition block from the OpenWeather API response.
/// </summary>
internal sealed class WeatherBlock
{
    public string? Icon { get; init; }
    public string? Description { get; init; }
}
