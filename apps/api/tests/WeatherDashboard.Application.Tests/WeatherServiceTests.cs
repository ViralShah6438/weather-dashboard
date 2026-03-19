using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Application.Services;
using WeatherDashboard.Domain.Exceptions;
using WeatherDashboard.Domain.Models;
using Xunit;

namespace WeatherDashboard.Application.Tests;

public sealed class WeatherServiceTests
{
    [Fact]
    public async Task GetByCityAsync_DelegatesToProvider()
    {
        var provider = new StubWeatherProvider();
        var service = new WeatherService(provider);

        var result = await service.GetByCityAsync("Berlin");

        Assert.Equal("Berlin", provider.LastCity);
        Assert.Equal("Berlin", result.City);
    }

    [Fact]
    public async Task GetByCityAsync_ThrowsForEmptyCity()
    {
        var provider = new StubWeatherProvider();
        var service = new WeatherService(provider);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByCityAsync(" "));
    }

    [Fact]
    public async Task GetByCityAsync_ThrowsForNullCity()
    {
        var provider = new StubWeatherProvider();
        var service = new WeatherService(provider);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByCityAsync(null!));
    }

    [Fact]
    public async Task GetByCityAsync_TrimsCityName()
    {
        var provider = new StubWeatherProvider();
        var service = new WeatherService(provider);

        await service.GetByCityAsync("  London  ");

        Assert.Equal("London", provider.LastCity);
    }

    [Fact]
    public async Task GetByCityAsync_PropagatesCityNotFoundException()
    {
        var provider = new ThrowingWeatherProvider(new CityNotFoundException("Unknown"));
        var service = new WeatherService(provider);

        await Assert.ThrowsAsync<CityNotFoundException>(() => service.GetByCityAsync("Unknown"));
    }

    [Fact]
    public async Task GetByCityAsync_PropagatesWeatherProviderException()
    {
        var provider = new ThrowingWeatherProvider(new WeatherProviderException("API Error"));
        var service = new WeatherService(provider);

        await Assert.ThrowsAsync<WeatherProviderException>(() => service.GetByCityAsync("London"));
    }

    [Fact]
    public async Task GetByCityAsync_ReturnsCorrectWeatherData()
    {
        var provider = new StubWeatherProvider();
        var service = new WeatherService(provider);

        var result = await service.GetByCityAsync("Tokyo");

        Assert.Equal("Tokyo", result.City);
        Assert.Equal(12m, result.TemperatureC);
        Assert.Equal(60, result.Humidity);
        Assert.Equal(5m, result.WindSpeedKph);
        Assert.Equal("01d", result.IconCode);
        Assert.Equal("clear sky", result.Description);
    }

    [Fact]
    public async Task GetByCityAsync_PassesCancellationToken()
    {
        var provider = new StubWeatherProvider();
        var service = new WeatherService(provider);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        await service.GetByCityAsync("Paris", token);

        Assert.Equal(token, provider.LastCancellationToken);
    }

    private sealed class StubWeatherProvider : IWeatherProvider
    {
        public string? LastCity { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
        {
            LastCity = city;
            LastCancellationToken = cancellationToken;
            return Task.FromResult(new WeatherSnapshot(city, 12, 60, 5, "01d", "clear sky"));
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
