namespace WeatherDashboard.Infrastructure.Clients.Models;

/// <summary>
/// Represents the root response payload from the OpenWeather Current Weather API.
/// </summary>
internal sealed class OpenWeatherPayload
{
    public string? Name { get; init; }
    public MainBlock? Main { get; init; }
    public WindBlock? Wind { get; init; }
    public List<WeatherBlock> Weather { get; init; } = [];
}
