namespace WeatherDashboard.Api.Contracts;

public sealed class WeatherResponse
{
    public required string City { get; init; }

    public required decimal TemperatureC { get; init; }

    public required int Humidity { get; init; }

    public required decimal WindSpeedKph { get; init; }

    public required string IconCode { get; init; }

    public required string Description { get; init; }
}
