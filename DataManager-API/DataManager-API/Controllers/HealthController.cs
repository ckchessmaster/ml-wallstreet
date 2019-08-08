using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataManagerAPI.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : Controller
    {
        // GET api/health
        [HttpGet]
        public IActionResult Get() // This method runs a health check and returns a JSON payload
        {
            return new JsonResult(new { Healthy = true });
        }
    }
}
