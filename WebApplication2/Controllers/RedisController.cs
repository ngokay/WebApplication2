using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using WebApplication2.Interface;

namespace WebApplication2.Controllers
{
    public class RedisController : Controller
    {
        private readonly IRedisService _redisService;

        public RedisController(IRedisService redisService)
        {
            _redisService = redisService;
        }

        [HttpPost("set-key")]
        public async Task<IActionResult> SetKey()
        {
            await _redisService.SetAsync("myKey", "Hello Redis", TimeSpan.FromMinutes(10));
            return Ok();
        }

        [HttpGet("get-key")]
        public async Task<IActionResult> GetKey()
        {
            var value = await _redisService.GetAsync("myKey");
            return Ok(value ?? "Test CICD ne ne ne");
        }
    }
}
