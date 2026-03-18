namespace WeatherDashboard.Application.Abstractions;

public interface IDefaultLocationService
{
    Task<string> GetAsync(CancellationToken cancellationToken = default);

    Task SetAsync(string city, CancellationToken cancellationToken = default);
}
