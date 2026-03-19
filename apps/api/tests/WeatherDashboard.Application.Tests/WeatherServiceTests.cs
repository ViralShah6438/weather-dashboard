using Moq;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Application.Services;
using WeatherDashboard.Domain.Exceptions;
using WeatherDashboard.Domain.Models;
using Xunit;

namespace WeatherDashboard.Application.Tests;

public sealed class WeatherServiceTests
{
    private readonly Mock<IWeatherProvider> _mockProvider;
    private readonly WeatherService _service;

    public WeatherServiceTests()
    {
        _mockProvider = new Mock<IWeatherProvider>();
        _service = new WeatherService(_mockProvider.Object);
    }

    [Fact]
    public async Task GetByCityAsync_DelegatesToProvider()
    {
        _mockProvider
            .Setup(p => p.GetCurrentAsync("Berlin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherSnapshot("Berlin", 12, 60, 5, "01d", "clear sky"));

        var result = await _service.GetByCityAsync("Berlin");

        Assert.Equal("Berlin", result.City);
        _mockProvider.Verify(p => p.GetCurrentAsync("Berlin", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByCityAsync_ThrowsForEmptyCity()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByCityAsync(" "));
    }

    [Fact]
    public async Task GetByCityAsync_ThrowsForNullCity()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByCityAsync(null!));
    }

    [Fact]
    public async Task GetByCityAsync_TrimsCityName()
    {
        _mockProvider
            .Setup(p => p.GetCurrentAsync("London", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherSnapshot("London", 12, 60, 5, "01d", "clear sky"));

        await _service.GetByCityAsync("  London  ");

        _mockProvider.Verify(p => p.GetCurrentAsync("London", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByCityAsync_PropagatesCityNotFoundException()
    {
        _mockProvider
            .Setup(p => p.GetCurrentAsync("Unknown", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CityNotFoundException("Unknown"));

        await Assert.ThrowsAsync<CityNotFoundException>(() => _service.GetByCityAsync("Unknown"));
    }

    [Fact]
    public async Task GetByCityAsync_PropagatesWeatherProviderException()
    {
        _mockProvider
            .Setup(p => p.GetCurrentAsync("London", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WeatherProviderException("API Error"));

        await Assert.ThrowsAsync<WeatherProviderException>(() => _service.GetByCityAsync("London"));
    }

    [Fact]
    public async Task GetByCityAsync_ReturnsCorrectWeatherData()
    {
        _mockProvider
            .Setup(p => p.GetCurrentAsync("Tokyo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherSnapshot("Tokyo", 12m, 60, 5m, "01d", "clear sky"));

        var result = await _service.GetByCityAsync("Tokyo");

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
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockProvider
            .Setup(p => p.GetCurrentAsync("Paris", token))
            .ReturnsAsync(new WeatherSnapshot("Paris", 12, 60, 5, "01d", "clear sky"));

        await _service.GetByCityAsync("Paris", token);

        _mockProvider.Verify(p => p.GetCurrentAsync("Paris", token), Times.Once);
    }
}
