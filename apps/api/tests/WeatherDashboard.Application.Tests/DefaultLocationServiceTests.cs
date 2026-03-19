using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Application.Services;
using WeatherDashboard.Domain.Exceptions;
using WeatherDashboard.Domain.Models;
using Xunit;

namespace WeatherDashboard.Application.Tests;

public sealed class DefaultLocationServiceTests
{
    [Fact]
    public async Task SetAsync_StoresTrimmedValue()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new FakeWeatherProvider();
        var service = new DefaultLocationService(repository, weatherProvider);

        await service.SetAsync("  Paris  ");

        var result = await service.GetAsync();
        Assert.Equal("Paris", result);
    }

    [Fact]
    public async Task SetAsync_ThrowsForEmptyCity()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new FakeWeatherProvider();
        var service = new DefaultLocationService(repository, weatherProvider);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SetAsync("  "));
    }

    [Fact]
    public async Task SetAsync_ThrowsForNullCity()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new FakeWeatherProvider();
        var service = new DefaultLocationService(repository, weatherProvider);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SetAsync(null!));
    }

    [Fact]
    public async Task SetAsync_ThrowsForInvalidCity()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new FakeWeatherProvider();
        var service = new DefaultLocationService(repository, weatherProvider);

        await Assert.ThrowsAsync<CityNotFoundException>(() => service.SetAsync("InvalidCityXYZ123"));
    }

    [Fact]
    public async Task SetAsync_UsesCanonicalCityName()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new FakeWeatherProvider();
        var service = new DefaultLocationService(repository, weatherProvider);

        // "paris" should be saved as "Paris" (canonical name from weather provider)
        await service.SetAsync("paris");

        var result = await service.GetAsync();
        Assert.Equal("Paris", result);
    }

    [Fact]
    public async Task SetAsync_DoesNotSaveWhenCityInvalid()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new FakeWeatherProvider();
        var service = new DefaultLocationService(repository, weatherProvider);

        var originalCity = await service.GetAsync();

        try
        {
            await service.SetAsync("InvalidCity123");
        }
        catch (CityNotFoundException)
        {
            // Expected
        }

        // Verify original value is preserved
        var currentCity = await service.GetAsync();
        Assert.Equal(originalCity, currentCity);
    }

    [Fact]
    public async Task SetAsync_PropagatesWeatherProviderException()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new ThrowingWeatherProvider(new WeatherProviderException("API Error"));
        var service = new DefaultLocationService(repository, weatherProvider);

        await Assert.ThrowsAsync<WeatherProviderException>(() => service.SetAsync("London"));
    }

    [Fact]
    public async Task GetAsync_ReturnsDefaultValue()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new FakeWeatherProvider();
        var service = new DefaultLocationService(repository, weatherProvider);

        var result = await service.GetAsync();

        Assert.Equal("London", result); // Default value in repository
    }

    [Fact]
    public async Task GetAsync_ReturnsUpdatedValueAfterSet()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new FakeWeatherProvider();
        var service = new DefaultLocationService(repository, weatherProvider);

        await service.SetAsync("Tokyo");
        var result = await service.GetAsync();

        Assert.Equal("Tokyo", result);
    }

    [Fact]
    public async Task SetAsync_PassesCancellationToken()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var weatherProvider = new FakeWeatherProvider();
        var service = new DefaultLocationService(repository, weatherProvider);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        await service.SetAsync("Paris", token);

        Assert.Equal(token, weatherProvider.LastCancellationToken);
        Assert.Equal(token, repository.LastCancellationToken);
    }

    private sealed class InMemoryDefaultLocationRepository : IDefaultLocationStoreCache
    {
        private string _city = "London";
        public CancellationToken LastCancellationToken { get; private set; }

        public Task<string> GetAsync(CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            return Task.FromResult(_city);
        }

        public Task SetAsync(string city, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            _city = city;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeWeatherProvider : IWeatherProvider
    {
        private static readonly HashSet<string> ValidCities = new(StringComparer.OrdinalIgnoreCase)
        {
            "Paris", "London", "New York", "Tokyo"
        };

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;

            if (!ValidCities.Contains(city))
            {
                throw new CityNotFoundException(city);
            }

            // Return canonical city name (proper casing)
            var canonicalName = ValidCities.First(c => c.Equals(city, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(new WeatherSnapshot(canonicalName, 20m, 50, 10m, "01d", "Clear sky"));
        }
    }

    private sealed class ThrowingWeatherProvider : IWeatherProvider
    {
        private readonly Exception _exception;

        public ThrowingWeatherProvider(Exception exception)
        {
            _exception = exception;
        }

        public Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
        {
            throw _exception;
        }
    }
}
