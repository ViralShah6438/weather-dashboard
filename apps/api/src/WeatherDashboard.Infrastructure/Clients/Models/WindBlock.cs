namespace WeatherDashboard.Infrastructure.Clients.Models;

/// <summary>
/// Represents the wind data block from the OpenWeather API response.
/// </summary>
internal sealed class WindBlock
{
    public decimal Speed { get; init; }
}
