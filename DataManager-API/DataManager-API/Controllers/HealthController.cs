using Microsoft.AspNetCore.Mvc;

namespace DataManager_API.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        // GET api/health
        [HttpGet]
        public IActionResult Get() // This method runs a health check and returns a JSON payload
        {
            return Content("{ \"healthy\":true}", "application/json");
        }
    }
}
