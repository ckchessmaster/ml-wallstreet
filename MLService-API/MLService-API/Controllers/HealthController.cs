using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MLServiceAPI.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        // GET api/health - returns health check in JSON format
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            return new JsonResult(new { Healthy = true });
        }
    }
}
