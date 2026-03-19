using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeatherDashboard.Api.Contracts;
using WeatherDashboard.Api.Controllers;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Exceptions;
using WeatherDashboard.Domain.Models;
using Xunit;

namespace WeatherDashboard.Api.Tests;

public sealed class WeatherControllerTests
{
    private readonly Mock<IWeatherService> _mockWeatherService;
    private readonly WeatherController _controller;

    public WeatherControllerTests()
    {
        _mockWeatherService = new Mock<IWeatherService>();
        _controller = new WeatherController(_mockWeatherService.Object, NullLogger<WeatherController>.Instance);
    }

    [Fact]
    public async Task GetByCity_ReturnsBadRequest_WhenCityIsEmpty()
    {
        var result = await _controller.GetByCity(" ", CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByCity_ReturnsBadRequest_WhenCityIsNull()
    {
        var result = await _controller.GetByCity(null!, CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByCity_ReturnsOk_WhenCityIsValid()
    {
        _mockWeatherService
            .Setup(s => s.GetByCityAsync("London", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherSnapshot("London", 15, 65, 12, "01d", "clear sky"));

        var result = await _controller.GetByCity("London", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetByCity_ReturnsCorrectWeatherData()
    {
        _mockWeatherService
            .Setup(s => s.GetByCityAsync("London", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherSnapshot("London", 15, 65, 12, "01d", "clear sky"));

        var result = await _controller.GetByCity("London", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<WeatherResponse>(ok.Value);
        Assert.Equal("London", response.City);
        Assert.Equal(15m, response.TemperatureC);
        Assert.Equal(65, response.Humidity);
        Assert.Equal(12m, response.WindSpeedKph);
        Assert.Equal("01d", response.IconCode);
        Assert.Equal("clear sky", response.Description);
    }

    [Fact]
    public async Task GetByCity_ReturnsNotFound_WhenCityNotFound()
    {
        _mockWeatherService
            .Setup(s => s.GetByCityAsync("Unknown", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CityNotFoundException("Unknown"));

        var result = await _controller.GetByCity("Unknown", CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task GetByCity_ReturnsBadGateway_WhenProviderFails()
    {
        _mockWeatherService
            .Setup(s => s.GetByCityAsync("London", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WeatherProviderException("API Error"));

        var result = await _controller.GetByCity("London", CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, problem.StatusCode);
    }
}
