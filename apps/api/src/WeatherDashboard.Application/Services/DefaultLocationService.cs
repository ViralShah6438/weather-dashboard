using WeatherDashboard.Application.Abstractions;

namespace WeatherDashboard.Application.Services;

public sealed class DefaultLocationService : IDefaultLocationService
{
    private readonly IDefaultLocationStoreCache _repository;

    public DefaultLocationService(IDefaultLocationStoreCache repository)
    {
        _repository = repository;
    }

    public Task<string> GetAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAsync(cancellationToken);
    }

    public Task SetAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City must be provided.", nameof(city));
        }

        return _repository.SetAsync(city.Trim(), cancellationToken);
    }
}
