using WeatherDashboard.Application.Abstractions;

namespace WeatherDashboard.Application.Services;

public sealed class DefaultLocationService : IDefaultLocationService
{
    private readonly IDefaultLocationStoreCache _repository;
    private readonly IWeatherProvider _weatherProvider;

    public DefaultLocationService(IDefaultLocationStoreCache repository, IWeatherProvider weatherProvider)
    {
        _repository = repository;
        _weatherProvider = weatherProvider;
    }

    public Task<string> GetAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAsync(cancellationToken);
    }

    public async Task SetAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City must be provided.", nameof(city));
        }

        var trimmedCity = city.Trim();

        // Validate that the city exists by attempting to fetch weather data
        // This will throw CityNotFoundException if the city is invalid
        var weather = await _weatherProvider.GetCurrentAsync(trimmedCity, cancellationToken);

        // Use the canonical city name returned by the weather provider
        await _repository.SetAsync(weather.City, cancellationToken);
    }
}
