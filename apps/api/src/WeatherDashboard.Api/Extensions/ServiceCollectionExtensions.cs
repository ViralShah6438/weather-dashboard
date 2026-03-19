using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Application.Services;
using WeatherDashboard.Infrastructure.Clients;
using WeatherDashboard.Infrastructure.Persistence;

namespace WeatherDashboard.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("FixedWindow", opt =>
            {
                opt.PermitLimit = configuration.GetValue("RateLimiting:FixedWindow:PermitLimit", 100);
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:FixedWindow:WindowSeconds", 60));
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = configuration.GetValue("RateLimiting:FixedWindow:QueueLimit", 10);
            });

            options.AddConcurrencyLimiter("Concurrency", opt =>
            {
                opt.PermitLimit = configuration.GetValue("RateLimiting:Concurrency:PermitLimit", 50);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = configuration.GetValue("RateLimiting:Concurrency:QueueLimit", 25);
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = configuration.GetValue("RateLimiting:Global:PermitLimit", 200),
                    Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Global:WindowSeconds", 60)),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = configuration.GetValue("RateLimiting:Global:QueueLimit", 5)
                });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    title = "Too Many Requests",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Rate limit exceeded. Please try again later."
                }, cancellationToken);
            };
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var isCacheEnabled = configuration.GetValue<bool>("IsCacheEnabled");
        if (isCacheEnabled)
        {
            services.AddScoped<IWeatherProvider, CachedWeatherProvider>();
        }
        else
        {
            services.AddScoped<IWeatherProvider, OpenWeatherClient>();
        }

        services.AddScoped<IWeatherService, WeatherService>();
        services.AddSingleton<IDefaultLocationStoreCache, DefaultLocationStoreCache>();
        services.AddScoped<IDefaultLocationService, DefaultLocationService>();

        return services;
    }
}
