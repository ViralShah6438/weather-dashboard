namespace WeatherDashboard.Domain.Models;

public sealed record WeatherSnapshot(
    string City,
    decimal TemperatureC,
    int Humidity,
    decimal WindSpeedKph,
    string IconCode,
    string Description);
