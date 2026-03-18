using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using WeatherDashboard.Application.Abstractions;
using WeatherDashboard.Application.Services;
using WeatherDashboard.Infrastructure.Clients;
using WeatherDashboard.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed Window Rate Limiter - limits requests per time window
    options.AddFixedWindowLimiter("FixedWindow", opt =>
    {
        opt.PermitLimit = builder.Configuration.GetValue("RateLimiting:FixedWindow:PermitLimit", 100);
        opt.Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:FixedWindow:WindowSeconds", 60));
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = builder.Configuration.GetValue("RateLimiting:FixedWindow:QueueLimit", 10);
    });

    // Concurrency Rate Limiter - limits concurrent requests
    options.AddConcurrencyLimiter("Concurrency", opt =>
    {
        opt.PermitLimit = builder.Configuration.GetValue("RateLimiting:Concurrency:PermitLimit", 50);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = builder.Configuration.GetValue("RateLimiting:Concurrency:QueueLimit", 25);
    });

    // Global rate limiter based on client IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("RateLimiting:Global:PermitLimit", 200),
            Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:Global:WindowSeconds", 60)),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = builder.Configuration.GetValue("RateLimiting:Global:QueueLimit", 5)
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.Configure<OpenWeatherOptions>(builder.Configuration.GetSection(OpenWeatherOptions.SectionName));
builder.Services.AddHttpClient<OpenWeatherClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenWeatherOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

var isCacheEnabled = builder.Configuration.GetValue<bool>("IsCacheEnabled");
if (isCacheEnabled)
{
    builder.Services.AddScoped<IWeatherProvider, CachedWeatherProvider>();
}
else
{
    builder.Services.AddScoped<IWeatherProvider, OpenWeatherClient>();
}

builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddSingleton<IDefaultLocationStoreCache, DefaultLocationStoreCache>();
builder.Services.AddScoped<IDefaultLocationService, DefaultLocationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors("AllowAngularApp");
app.MapControllers();

app.Run();

