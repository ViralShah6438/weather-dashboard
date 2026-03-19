using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Models;

namespace WeatherDashboard.Infrastructure.Clients;

public sealed class CachedWeatherProvider : IWeatherProvider
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly IMemoryCache _memoryCache;
    private readonly OpenWeatherClient _inner;
    private readonly ILogger<CachedWeatherProvider> _logger;

    public CachedWeatherProvider(IMemoryCache memoryCache, OpenWeatherClient inner, ILogger<CachedWeatherProvider> logger)
    {
        _memoryCache = memoryCache;
        _inner = inner;
        _logger = logger;
    }

    public Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
    {
        var key = $"weather:{city.Trim().ToLowerInvariant()}";
        
        if (_memoryCache.TryGetValue<WeatherSnapshot>(key, out var cached) && cached is not null)
        {
            _logger.LogDebug("Cache hit for city: {City}", city);
            return Task.FromResult(cached);
        }

        _logger.LogDebug("Cache miss for city: {City}, fetching from API", city);
        
        return _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _inner.GetCurrentAsync(city, cancellationToken);
        })!;
    }
}
