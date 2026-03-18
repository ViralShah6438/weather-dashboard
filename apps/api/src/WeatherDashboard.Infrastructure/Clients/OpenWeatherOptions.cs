namespace WeatherDashboard.Infrastructure.Clients;

public sealed class OpenWeatherOptions
{
    public const string SectionName = "OpenWeather";

    public string BaseUrl { get; init; } = "https://api.openweathermap.org";

    public string ApiKey { get; init; } = string.Empty;
}
