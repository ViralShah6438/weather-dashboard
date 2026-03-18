using WeatherDashboard.Domain.Models;

namespace WeatherDashboard.Application.Abstractions;

public interface IWeatherProvider
{
    Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken = default);
}

