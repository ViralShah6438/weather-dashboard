using Moq;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Application.Services;
using WeatherDashboard.Domain.Exceptions;
using WeatherDashboard.Domain.Models;
using Xunit;

namespace WeatherDashboard.Application.Tests;

public sealed class DefaultLocationServiceTests
{
    private readonly Mock<IDefaultLocationStoreCache> _mockRepository;
    private readonly Mock<IWeatherProvider> _mockWeatherProvider;
    private readonly DefaultLocationService _service;

    public DefaultLocationServiceTests()
    {
        _mockRepository = new Mock<IDefaultLocationStoreCache>();
        _mockWeatherProvider = new Mock<IWeatherProvider>();
        _service = new DefaultLocationService(_mockRepository.Object, _mockWeatherProvider.Object);
    }

    [Fact]
    public async Task SetAsync_StoresTrimmedValue()
    {
        _mockWeatherProvider
            .Setup(p => p.GetCurrentAsync("Paris", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherSnapshot("Paris", 20m, 50, 10m, "01d", "Clear sky"));
        _mockRepository
            .Setup(r => r.SetAsync("Paris", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SetAsync("  Paris  ");

        _mockRepository.Verify(r => r.SetAsync("Paris", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ThrowsForEmptyCity()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SetAsync("  "));
    }

    [Fact]
    public async Task SetAsync_ThrowsForNullCity()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SetAsync(null!));
    }

    [Fact]
    public async Task SetAsync_ThrowsForInvalidCity()
    {
        _mockWeatherProvider
            .Setup(p => p.GetCurrentAsync("InvalidCityXYZ123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CityNotFoundException("InvalidCityXYZ123"));

        await Assert.ThrowsAsync<CityNotFoundException>(() => _service.SetAsync("InvalidCityXYZ123"));
    }

    [Fact]
    public async Task SetAsync_UsesCanonicalCityName()
    {
        // Weather provider returns canonical name "Paris" even when searching "paris"
        _mockWeatherProvider
            .Setup(p => p.GetCurrentAsync("paris", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherSnapshot("Paris", 20m, 50, 10m, "01d", "Clear sky"));
        _mockRepository
            .Setup(r => r.SetAsync("Paris", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SetAsync("paris");

        // Should save canonical name "Paris" not "paris"
        _mockRepository.Verify(r => r.SetAsync("Paris", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_DoesNotSaveWhenCityInvalid()
    {
        _mockWeatherProvider
            .Setup(p => p.GetCurrentAsync("InvalidCity123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CityNotFoundException("InvalidCity123"));

        try
        {
            await _service.SetAsync("InvalidCity123");
        }
        catch (CityNotFoundException)
        {
            // Expected
        }

        // Verify SetAsync was never called on repository
        _mockRepository.Verify(r => r.SetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetAsync_PropagatesWeatherProviderException()
    {
        _mockWeatherProvider
            .Setup(p => p.GetCurrentAsync("London", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WeatherProviderException("API Error"));

        await Assert.ThrowsAsync<WeatherProviderException>(() => _service.SetAsync("London"));
    }

    [Fact]
    public async Task GetAsync_ReturnsValueFromRepository()
    {
        _mockRepository
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("London");

        var result = await _service.GetAsync();

        Assert.Equal("London", result);
    }

    [Fact]
    public async Task GetAsync_ReturnsUpdatedValueAfterSet()
    {
        _mockWeatherProvider
            .Setup(p => p.GetCurrentAsync("Tokyo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherSnapshot("Tokyo", 20m, 50, 10m, "01d", "Clear sky"));
        _mockRepository
            .Setup(r => r.SetAsync("Tokyo", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Tokyo");

        await _service.SetAsync("Tokyo");
        var result = await _service.GetAsync();

        Assert.Equal("Tokyo", result);
    }

    [Fact]
    public async Task SetAsync_PassesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockWeatherProvider
            .Setup(p => p.GetCurrentAsync("Paris", token))
            .ReturnsAsync(new WeatherSnapshot("Paris", 20m, 50, 10m, "01d", "Clear sky"));
        _mockRepository
            .Setup(r => r.SetAsync("Paris", token))
            .Returns(Task.CompletedTask);

        await _service.SetAsync("Paris", token);

        _mockWeatherProvider.Verify(p => p.GetCurrentAsync("Paris", token), Times.Once);
        _mockRepository.Verify(r => r.SetAsync("Paris", token), Times.Once);
    }
}
