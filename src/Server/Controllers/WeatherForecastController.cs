using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly object syncLock = new();
    private static int connectionCount = 0;
    private static int maxConnections = 0;
    private static int totalRequests = 0;

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("callback")]
    public object GetThreadCount()
    {
        ThreadPool.GetMinThreads(out int minW, out int minIo);
        ThreadPool.GetMaxThreads(out int maxW, out int maxIo);
        ThreadPool.GetAvailableThreads(out int avW, out int avIo);

        return new
        {
            MinWorker = minW,
            MinIo = minIo,
            MaxWorker = maxW,
            MaxIo = maxIo,
            AvailableWorker = avW,
            AvailableIo = avIo,
            ConnectionCount = connectionCount,
            MaxConnections = maxConnections,
            TotalRequests = totalRequests
        };
    }

    [HttpPost("callback")]
    public async Task<IActionResult> Callback()
    {
        const int delayLength = 5000;

        int currentCount = -1;
        int max = -1;
        int requestsSoFar = -1;

        lock (syncLock)
        {
            currentCount = ++connectionCount;
            max = maxConnections = Math.Max(maxConnections, currentCount);
            requestsSoFar = ++totalRequests;
        }
        
        await Task.Delay(delayLength);

        Interlocked.Decrement(ref connectionCount);

        _logger.LogInformation("\tConnection count: {ConnectionCount}\n\tMax connections: {maxConnections}\n\tRequests so far: {totalRequests}",
            currentCount, max, requestsSoFar);

        return this.Ok();
    }
}
