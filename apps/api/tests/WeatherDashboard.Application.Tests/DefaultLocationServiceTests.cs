using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Application.Services;
using Xunit;

namespace WeatherDashboard.Application.Tests;

public sealed class DefaultLocationServiceTests
{
    [Fact]
    public async Task SetAsync_StoresTrimmedValue()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var service = new DefaultLocationService(repository);

        service.Set("  Paris  ");

        Assert.Equal("Paris", service.Get());
    }

    [Fact]
    public async Task SetAsync_ThrowsForEmptyCity()
    {
        var repository = new InMemoryDefaultLocationRepository();
        var service = new DefaultLocationService(repository);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SetAsync("  "));
    }

    private sealed class InMemoryDefaultLocationRepository : IDefaultLocationStoreCache
    {
        private string _city = "London";

        public string Get(CancellationToken cancellationToken = default)
        {
            return _city;
        }

        public void Set(string city, CancellationToken cancellationToken = default)
         {
            _city = city;
         }
     }
 }
