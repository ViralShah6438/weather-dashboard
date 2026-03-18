namespace WeatherDashboard.Domain.Exceptions;

public sealed class WeatherProviderException : Exception
{
    public WeatherProviderException(string message)
        : base(message)
    {
    }
}
