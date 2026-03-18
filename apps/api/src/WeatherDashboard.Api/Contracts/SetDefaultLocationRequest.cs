namespace WeatherDashboard.Api.Contracts;

public sealed class SetDefaultLocationRequest
{
    public string City { get; init; } = string.Empty;
}
