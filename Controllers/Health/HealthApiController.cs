using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EmailTriggerApp.Controllers.Health
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthApiController : ControllerBase
    {
        // GET: api/<HealthController>
        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> get()
        {
            // Simulate some async work if needed
            await Task.Delay(100);
            return Ok("Health check passed");
        }

    }
}
