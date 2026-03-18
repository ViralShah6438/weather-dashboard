namespace WeatherDashboard.Infrastructure.Clients.Models;

/// <summary>
/// Represents a geographic location returned by the OpenWeather Geocoding API.
/// </summary>
internal sealed class GeoLocation
{
    public string? Name { get; init; }
    public decimal Lat { get; init; }
    public decimal Lon { get; init; }
    public string? Country { get; init; }
}
