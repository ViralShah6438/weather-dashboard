namespace WeatherDashboard.Infrastructure.Clients.Models;

/// <summary>
/// Represents the main weather data block from the OpenWeather API response.
/// </summary>
internal sealed class MainBlock
{
    public decimal Temp { get; init; }
    public int Humidity { get; init; }
}
