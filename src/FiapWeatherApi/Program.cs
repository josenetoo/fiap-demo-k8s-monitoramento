using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck("database", () => HealthCheckResult.Healthy("Simulated DB connection"));

// OpenTelemetry Configuration - Versão Simplificada
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource("FiapWeatherApi")
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("fiap-weather-api", "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() ?? "development"
                }))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter() // Para debug
            .AddOtlpExporter()) // Configuração automática via variáveis de ambiente
    .WithMetrics(meterProviderBuilder =>
        meterProviderBuilder
            .AddMeter("FiapWeatherApi")
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("fiap-weather-api", "1.0.0"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure middleware
app.UseRouting();
app.UseHttpMetrics(); // Prometheus.NET middleware

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Prometheus metrics endpoint (apenas um)
app.MapMetrics(); // /metrics endpoint do Prometheus.NET

app.Run();
