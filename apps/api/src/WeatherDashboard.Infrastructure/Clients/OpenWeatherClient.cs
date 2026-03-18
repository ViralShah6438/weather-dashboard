using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Domain.Exceptions;
using WeatherDashboard.Domain.Models;
using WeatherDashboard.Infrastructure.Clients.Models;

namespace WeatherDashboard.Infrastructure.Clients;

public sealed class OpenWeatherClient : IWeatherProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenWeatherOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public OpenWeatherClient(HttpClient httpClient, IOptions<OpenWeatherOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<WeatherSnapshot> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new WeatherProviderException("OpenWeather API key is not configured.");
        }

        // Step 1: Use Geocoding API to convert city name to coordinates
        var geoLocation = await GetGeoLocationAsync(city, cancellationToken);

        // Step 2: Use Weather API with coordinates
        var weatherUri = $"/data/2.5/weather?lat={geoLocation.Lat}&lon={geoLocation.Lon}&units=metric&appid={_options.ApiKey}";
        using var weatherResponse = await _httpClient.GetAsync(weatherUri, cancellationToken);

        if (!weatherResponse.IsSuccessStatusCode)
        {
            throw new WeatherProviderException($"OpenWeather request failed with status {(int)weatherResponse.StatusCode}.");
        }

        await using var contentStream = await weatherResponse.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<OpenWeatherPayload>(contentStream, JsonOptions, cancellationToken);

        if (payload is null || payload.Main is null || payload.Wind is null || payload.Weather is null || payload.Weather.Count == 0)
        {
            throw new WeatherProviderException("OpenWeather response was invalid.");
        }

        var primaryWeather = payload.Weather[0];
        return new WeatherSnapshot(
            geoLocation.Name ?? city,
            payload.Main.Temp,
            payload.Main.Humidity,
            payload.Wind.Speed * 3.6m,
            primaryWeather.Icon ?? "01d",
            primaryWeather.Description ?? "N/A");
    }

    private async Task<GeoLocation> GetGeoLocationAsync(string city, CancellationToken cancellationToken)
    {
        var geoUri = $"/geo/1.0/direct?q={Uri.EscapeDataString(city)}&limit=1&appid={_options.ApiKey}";
        using var geoResponse = await _httpClient.GetAsync(geoUri, cancellationToken);

        if (!geoResponse.IsSuccessStatusCode)
        {
            throw new WeatherProviderException($"OpenWeather Geocoding request failed with status {(int)geoResponse.StatusCode}.");
        }

        await using var geoStream = await geoResponse.Content.ReadAsStreamAsync(cancellationToken);
        var geoResults = await JsonSerializer.DeserializeAsync<List<GeoLocation>>(geoStream, JsonOptions, cancellationToken);

        if (geoResults is null || geoResults.Count == 0)
        {
            throw new CityNotFoundException(city);
        }

        return geoResults[0];
    }
}

