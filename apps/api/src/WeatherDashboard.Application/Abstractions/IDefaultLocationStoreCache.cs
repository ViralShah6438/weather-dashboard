namespace WeatherDashboard.Application.Abstractions;

public interface IDefaultLocationStoreCache
{
    Task<string> GetAsync(CancellationToken cancellationToken = default);

    Task SetAsync(string city, CancellationToken cancellationToken = default);
}
