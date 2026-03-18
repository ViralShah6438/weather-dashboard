using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using WeatherDashboard.Application.Abstractions;

namespace WeatherDashboard.Infrastructure.Persistence;

public sealed class DefaultLocationStoreCache : IDefaultLocationStoreCache
{
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private string defaultLocation = "London";

    public async Task<string> GetAsync(CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            if (string.IsNullOrWhiteSpace(defaultLocation))
            {
                return "London";
            }

            return defaultLocation;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task SetAsync(string city, CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            defaultLocation = city;
        }
        finally
        {
            _mutex.Release();
        }
    }
}

