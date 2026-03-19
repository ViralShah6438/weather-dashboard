using Microsoft.Extensions.Http.Resilience;
using Polly;
using WeatherDashboard.Infrastructure.Clients;

namespace WeatherDashboard.Api.Extensions;

public static class ResilienceExtensions
{
    public static IServiceCollection AddOpenWeatherClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenWeatherOptions>(configuration.GetSection(OpenWeatherOptions.SectionName));

        services.AddHttpClient<OpenWeatherClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenWeatherOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(configuration.GetValue("Resilience:HttpClient:TimeoutSeconds", 30));
        })
        .AddResilienceHandler("OpenWeatherCircuitBreaker", (resilienceBuilder, context) =>
        {
            var config = context.ServiceProvider.GetRequiredService<IConfiguration>();
            ConfigureCircuitBreaker(resilienceBuilder, config, context.ServiceProvider);
        });

        return services;
    }

    private static void ConfigureCircuitBreaker(
        ResiliencePipelineBuilder<HttpResponseMessage> builder,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        var samplingDuration = configuration.GetValue("Resilience:CircuitBreaker:SamplingDurationSeconds", 30);
        var failureRatio = configuration.GetValue("Resilience:CircuitBreaker:FailureRatio", 0.5);
        var minimumThroughput = configuration.GetValue("Resilience:CircuitBreaker:MinimumThroughput", 3);
        var breakDuration = configuration.GetValue("Resilience:CircuitBreaker:BreakDurationSeconds", 30);

        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("CircuitBreaker");

        builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            SamplingDuration = TimeSpan.FromSeconds(samplingDuration),
            FailureRatio = failureRatio,
            MinimumThroughput = minimumThroughput,
            BreakDuration = TimeSpan.FromSeconds(breakDuration),
            ShouldHandle = args =>
            {
                // Handle exceptions (network errors, timeouts)
                if (args.Outcome.Exception is not null)
                {
                    logger?.LogDebug(
                        "Circuit breaker evaluating exception: {ExceptionType} - {Message}",
                        args.Outcome.Exception.GetType().Name,
                        args.Outcome.Exception.Message);
                    return ValueTask.FromResult(args.Outcome.Exception is HttpRequestException or TaskCanceledException);
                }

                // Handle HTTP response errors
                if (args.Outcome.Result is HttpResponseMessage response)
                {
                    var statusCode = (int)response.StatusCode;
                    // Handle 5xx server errors, 429 too many requests, 408 timeout
                    var shouldHandle = statusCode >= 500 || statusCode == 429 || statusCode == 408;
                    
                    logger?.LogDebug(
                        "Circuit breaker evaluating response: {StatusCode}, ShouldHandle: {ShouldHandle}",
                        statusCode,
                        shouldHandle);
                    
                    return ValueTask.FromResult(shouldHandle);
                }

                return ValueTask.FromResult(false);
            },
            OnOpened = args =>
            {
                logger?.LogWarning(
                    "Circuit breaker OPENED for OpenWeather API. Break duration: {BreakDuration}s. Reason: {Reason}",
                    args.BreakDuration.TotalSeconds,
                    args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString() ?? "Unknown");
                return ValueTask.CompletedTask;
            },
            OnClosed = _ =>
            {
                logger?.LogInformation("Circuit breaker CLOSED for OpenWeather API. Service recovered.");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = _ =>
            {
                logger?.LogInformation("Circuit breaker HALF-OPENED for OpenWeather API. Testing service health.");
                return ValueTask.CompletedTask;
            }
        });
    }
}
