using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data.Entity;
using System.Diagnostics;
using WebApplication2.Data;
using WebApplication2.Data.Model;
using WebApplication2.Interface;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IRedisService _redisService;
        public static readonly ActivitySource ActivitySource = new("my-dotnet-service");
        private readonly ITracingService _tracer;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, AppDbContext context, IRedisService redisService, ITracingService tracer)
        {
            _logger = logger;
            _context = context;
            _redisService = redisService;
            _tracer = tracer;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            await _tracer.TraceAsync("Step1", () => Task.Delay(100));
            await _tracer.TraceAsync("Step2", () => Task.Delay(100));
            await _tracer.TraceAsync("Step3", () => Task.Delay(100));
            await _tracer.TraceAsync("Step4", () => Task.Delay(100));
            await _tracer.TraceAsync("Step5", () => Task.Delay(100));
            
            return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
        }

        [HttpGet("db")]
        public async Task<IActionResult> QueryDb()
        {
            _logger.LogInformation("📌 Đây là log thông tin gửi lên SigNoz tại {Time}", DateTime.UtcNow);
            _logger.LogWarning("⚠️ Đây là log cảnh báo");

            var users = _tracer.Trace("DB: Load Users", () => _context.Users.ToList());

            var existKey = await _tracer.TraceAsync("Redis: Get myKey", () => _redisService.GetAsync("myKey"));

            return string.IsNullOrEmpty(existKey)
                ? Ok("Không tồn tại key")
                : Ok(users);
        }
    }
}
