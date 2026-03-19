using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Api.Contracts;
using WeatherDashboard.Api.Controllers;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Exceptions;
using WeatherDashboard.Domain.Models;
using Xunit;

namespace WeatherDashboard.Api.Tests;

public sealed class WeatherControllerTests
{
    [Fact]
    public async Task GetByCity_ReturnsBadRequest_WhenCityIsEmpty()
    {
        var controller = new WeatherController(new StubWeatherService());

        var result = await controller.GetByCity(" ", CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByCity_ReturnsBadRequest_WhenCityIsNull()
    {
        var controller = new WeatherController(new StubWeatherService());

        var result = await controller.GetByCity(null!, CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByCity_ReturnsOk_WhenCityIsValid()
    {
        var controller = new WeatherController(new StubWeatherService());

        var result = await controller.GetByCity("London", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetByCity_ReturnsCorrectWeatherData()
    {
        var controller = new WeatherController(new StubWeatherService());

        var result = await controller.GetByCity("London", CancellationToken.None);

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
        var service = new ThrowingWeatherService(new CityNotFoundException("Unknown"));
        var controller = new WeatherController(service);

        var result = await controller.GetByCity("Unknown", CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task GetByCity_ReturnsBadGateway_WhenProviderFails()
    {
        var service = new ThrowingWeatherService(new WeatherProviderException("API Error"));
        var controller = new WeatherController(service);

        var result = await controller.GetByCity("London", CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, problem.StatusCode);
    }

    private sealed class StubWeatherService : IWeatherService
    {
        public Task<WeatherSnapshot> GetByCityAsync(string city, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new WeatherSnapshot(city, 15, 65, 12, "01d", "clear sky"));
        }
    }

    private sealed class ThrowingWeatherService : IWeatherService
    {
        private readonly Exception _exception;

        public ThrowingWeatherService(Exception exception)
        {
            _exception = exception;
        }

        public Task<WeatherSnapshot> GetByCityAsync(string city, CancellationToken cancellationToken = default)
        {
            throw _exception;
        }
    }
}

public sealed class SettingsControllerTests
{
    [Fact]
    public async Task GetDefaultLocation_ReturnsOk_WithCity()
    {
        var service = new StubDefaultLocationService();
        var controller = new SettingsController(service);

        var result = await controller.GetDefaultLocation(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DefaultLocationResponse>(ok.Value);
        Assert.Equal("London", response.City);
    }

    [Fact]
    public async Task SetDefaultLocation_ReturnsNoContent_WhenValid()
    {
        var service = new StubDefaultLocationService();
        var controller = new SettingsController(service);

        var result = await controller.SetDefaultLocation(new SetDefaultLocationRequest
        {
            City = "Paris"
        }, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Paris", await service.GetAsync());
    }

    [Fact]
    public async Task SetDefaultLocation_ReturnsBadRequest_WhenCityIsEmpty()
    {
        var service = new StubDefaultLocationService();
        var controller = new SettingsController(service);

        var result = await controller.SetDefaultLocation(new SetDefaultLocationRequest
        {
            City = "  "
        }, CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task SetDefaultLocation_ReturnsBadRequest_WhenCityIsNull()
    {
        var service = new StubDefaultLocationService();
        var controller = new SettingsController(service);

        var result = await controller.SetDefaultLocation(new SetDefaultLocationRequest
        {
            City = null!
        }, CancellationToken.None);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task SetDefaultLocation_ReturnsNotFound_WhenCityInvalid()
    {
        var service = new ThrowingDefaultLocationService(new CityNotFoundException("Unknown"));
        var controller = new SettingsController(service);

        var result = await controller.SetDefaultLocation(new SetDefaultLocationRequest
        {
            City = "InvalidCity123"
        }, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task SetDefaultLocation_PreservesOriginalValue_WhenCityInvalid()
    {
        var service = new StubDefaultLocationService();
        var controller = new SettingsController(service);
        var originalCity = await service.GetAsync();

        // Simulate invalid city by using throwing service
        var throwingService = new ThrowingDefaultLocationService(new CityNotFoundException("Unknown"));
        var throwingController = new SettingsController(throwingService);

        await throwingController.SetDefaultLocation(new SetDefaultLocationRequest
        {
            City = "InvalidCity123"
        }, CancellationToken.None);

        // Original service should still have original value
        Assert.Equal(originalCity, await service.GetAsync());
    }

    private sealed class StubDefaultLocationService : IDefaultLocationService
    {
        private string _city = "London";

        public Task<string> GetAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_city);
        }

        public Task SetAsync(string city, CancellationToken cancellationToken = default)
        {
            _city = city;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingDefaultLocationService : IDefaultLocationService
    {
        private readonly Exception _exception;
        private string _city = "London";

        public ThrowingDefaultLocationService(Exception exception)
        {
            _exception = exception;
        }

        public Task<string> GetAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_city);
        }

        public Task SetAsync(string city, CancellationToken cancellationToken = default)
        {
            throw _exception;
        }
    }
}
