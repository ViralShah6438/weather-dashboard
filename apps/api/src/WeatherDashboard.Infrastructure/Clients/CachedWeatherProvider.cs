using Microsoft.Extensions.Caching.Memory;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Models;

namespace WeatherDashboard.Infrastructure.Clients;

public sealed class CachedWeatherProvider : IWeatherProvider
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly IMemoryCache _memoryCache;
    private readonly OpenWeatherClient _inner;

    public CachedWeatherProvider(IMemoryCache memoryCache, OpenWeatherClient inner)
    {
        _memoryCache = memoryCache;
        _inner = inner;
    }

    public Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
    {
        var key = $"weather:{city.Trim().ToLowerInvariant()}";
        return _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _inner.GetCurrentAsync(city, cancellationToken);
        })!;
    }
}
