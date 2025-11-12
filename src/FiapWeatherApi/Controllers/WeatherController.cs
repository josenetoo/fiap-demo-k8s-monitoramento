using Microsoft.AspNetCore.Mvc;
using Prometheus;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FiapWeatherApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherController> _logger;
    
    // Prometheus.NET Metrics
    private static readonly Counter RequestCounter = 
        Metrics.CreateCounter("weather_requests_total", "Total weather requests", new[] { "method", "endpoint" });
    
    private static readonly Histogram RequestDuration = 
        Metrics.CreateHistogram("weather_request_duration_seconds", "Weather request duration");
    
    private static readonly Gauge ActiveRequests = 
        Metrics.CreateGauge("weather_active_requests", "Currently active weather requests");

    // OpenTelemetry Metrics
    private static readonly ActivitySource ActivitySource = new("FiapWeatherApi");
    private static readonly Meter Meter = new("FiapWeatherApi");
    private static readonly Counter<long> OtelRequestCounter = Meter.CreateCounter<long>("weather_requests_otel_total");
    private static readonly Histogram<double> OtelRequestDuration = Meter.CreateHistogram<double>("weather_request_otel_duration_ms");

    public WeatherController(ILogger<WeatherController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        using var activity = ActivitySource.StartActivity("GetWeatherForecast");
        using var timer = RequestDuration.NewTimer();
        
        ActiveRequests.Inc();
        RequestCounter.WithLabels("GET", "/api/weather").Inc();
        OtelRequestCounter.Add(1, new("method", "GET"), new("endpoint", "/api/weather"));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Processing weather forecast request");
            
            // Simular latência variável para demonstração
            var delay = Random.Shared.Next(50, 300);
            await Task.Delay(delay);
            
            // Simular erro ocasional (5% das vezes)
            if (Random.Shared.Next(1, 21) == 1)
            {
                _logger.LogError("Simulated error in weather service");
                throw new InvalidOperationException("Weather service temporarily unavailable");
            }

            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();

            activity?.SetTag("forecast.count", forecasts.Length);
            _logger.LogInformation("Successfully generated {Count} weather forecasts", forecasts.Length);

            return forecasts;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error generating weather forecast");
            throw;
        }
        finally
        {
            ActiveRequests.Dec();
            stopwatch.Stop();
            OtelRequestDuration.Record(stopwatch.ElapsedMilliseconds, 
                new("method", "GET"), 
                new("endpoint", "/api/weather"));
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        using var activity = ActivitySource.StartActivity("HealthCheck");
        
        RequestCounter.WithLabels("GET", "/api/weather/health").Inc();
        
        var healthData = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        _logger.LogInformation("Health check requested - Status: {Status}", healthData.Status);
        
        return Ok(healthData);
    }

    [HttpGet("metrics-demo")]
    public IActionResult MetricsDemo()
    {
        using var activity = ActivitySource.StartActivity("MetricsDemo");
        
        // Incrementar contadores para demonstração
        for (int i = 0; i < Random.Shared.Next(1, 10); i++)
        {
            RequestCounter.WithLabels("DEMO", "/api/weather/metrics-demo").Inc();
            OtelRequestCounter.Add(1, new("method", "DEMO"), new("endpoint", "/metrics-demo"));
        }

        var metricsInfo = new
        {
            Message = "Metrics demonstration endpoint",
            PrometheusEndpoint = "/metrics",
            HealthEndpoint = "/health",
            RequestsGenerated = "Check Prometheus for updated metrics"
        };

        _logger.LogInformation("Metrics demo endpoint called");
        
        return Ok(metricsInfo);
    }
}

public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}
