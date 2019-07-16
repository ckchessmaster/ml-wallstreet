using Microsoft.AspNetCore.Mvc;

namespace MLService_API.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        // GET api/health - returns health check in JSON format
        [HttpGet]
        public IActionResult Get()
        {
            return Content("{ \"healthy\":true}", "application/json");
        }
    }
}
