using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeatherDashboard.Api.Contracts;
using WeatherDashboard.Api.Controllers;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Exceptions;
using Xunit;

namespace WeatherDashboard.Api.Tests
{
    public sealed class SettingsControllerTests
    {
        private readonly Mock<IDefaultLocationService> _mockService;
        private readonly SettingsController _controller;

        public SettingsControllerTests()
        {
            _mockService = new Mock<IDefaultLocationService>();
            _controller = new SettingsController(_mockService.Object, NullLogger<SettingsController>.Instance);
        }

        [Fact]
        public async Task GetDefaultLocation_ReturnsOk_WithCity()
        {
            _mockService
                .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("London");

            var result = await _controller.GetDefaultLocation(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DefaultLocationResponse>(ok.Value);
            Assert.Equal("London", response.City);
        }

        [Fact]
        public async Task SetDefaultLocation_ReturnsNoContent_WhenValid()
        {
            _mockService
                .Setup(s => s.SetAsync("Paris", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.SetDefaultLocation(new SetDefaultLocationRequest
            {
                City = "Paris"
            }, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(s => s.SetAsync("Paris", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SetDefaultLocation_ReturnsBadRequest_WhenCityIsEmpty()
        {
            var result = await _controller.SetDefaultLocation(new SetDefaultLocationRequest
            {
                City = "  "
            }, CancellationToken.None);

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task SetDefaultLocation_ReturnsBadRequest_WhenCityIsNull()
        {
            var result = await _controller.SetDefaultLocation(new SetDefaultLocationRequest
            {
                City = null!
            }, CancellationToken.None);

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task SetDefaultLocation_ReturnsNotFound_WhenCityInvalid()
        {
            _mockService
                .Setup(s => s.SetAsync("InvalidCity123", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new CityNotFoundException("InvalidCity123"));

            var result = await _controller.SetDefaultLocation(new SetDefaultLocationRequest
            {
                City = "InvalidCity123"
            }, CancellationToken.None);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task SetDefaultLocation_DoesNotCallSet_WhenCityIsEmpty()
        {
            await _controller.SetDefaultLocation(new SetDefaultLocationRequest
            {
                City = "  "
            }, CancellationToken.None);

            _mockService.Verify(s => s.SetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
