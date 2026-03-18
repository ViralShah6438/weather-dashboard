using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Application.Services;
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

    private sealed class StubWeatherProvider : IWeatherProvider
    {
        public string? LastCity { get; private set; }

        public Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
        {
            LastCity = city;
            return Task.FromResult(new WeatherSnapshot(city, 12, 60, 5, "01d", "clear sky"));
        }
    }
}
