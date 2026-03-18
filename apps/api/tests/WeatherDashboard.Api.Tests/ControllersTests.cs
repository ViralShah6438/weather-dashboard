using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Api.Controllers;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Models;
using Xunit;

namespace WeatherDashboard.Api.Tests;

public sealed class ControllersTests
{
    [Fact]
    public async Task WeatherController_ReturnsBadRequest_WhenCityMissing()
    {
        var controller = new WeatherController(new StubWeatherService());

        var result = await controller.GetByCity(" ", CancellationToken.None);

        Assert.IsType<ObjectResult>(result);
    }

    [Fact]
    public async Task WeatherController_ReturnsWeather_WhenCityValid()
    {
        var controller = new WeatherController(new StubWeatherService());

        var result = await controller.GetByCity("London", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task SettingsController_ReturnsNoContent_OnValidUpdate()
    {
        var service = new StubDefaultLocationService();
        var controller = new SettingsController(service);

        var result = await controller.SetDefaultLocation(new WeatherDashboard.Api.Contracts.SetDefaultLocationRequest
        {
            City = "Paris"
        }, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Paris", await service.GetAsync());
    }

    private sealed class StubWeatherService : IWeatherService
    {
        public Task<WeatherSnapshot> GetByCityAsync(string city, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new WeatherSnapshot(city, 15, 65, 12, "01d", "clear sky"));
        }
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
}
