using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Models;

namespace WeatherDashboard.Application.Services;

public sealed class WeatherService : IWeatherService
{
    private readonly IWeatherProvider _weatherProvider;

    public WeatherService(IWeatherProvider weatherProvider)
    {
        _weatherProvider = weatherProvider;
    }

    public Task<WeatherSnapshot> GetByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City must be provided.", nameof(city));
        }

        return _weatherProvider.GetCurrentAsync(city.Trim(), cancellationToken);
    }
}

