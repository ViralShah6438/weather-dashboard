using System.Threading.RateLimiting;
using WeatherDashboard.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Application Insights (only if connection string is provided)
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

// Configure services
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddRateLimitingPolicies(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddOpenWeatherClient(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
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

