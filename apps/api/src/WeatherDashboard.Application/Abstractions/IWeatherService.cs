using WeatherDashboard.Domain.Models;

namespace WeatherDashboard.Application.Abstractions;

public interface IWeatherService
{
    Task<WeatherSnapshot> GetByCityAsync(string city, CancellationToken cancellationToken = default);
}
