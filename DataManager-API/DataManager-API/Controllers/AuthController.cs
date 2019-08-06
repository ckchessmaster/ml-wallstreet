using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MLWCore.Security;
using Microsoft.Extensions.Configuration;

namespace DataManagerAPI.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration config;

        public AuthController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpGet]
        [Route("getToken")]
        public IActionResult GetToken([FromHeader(Name = "Api-Key")]string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return new BadRequestResult();
            }



            return new JsonResult(new { Token = apiKey });
        }
    }
}
