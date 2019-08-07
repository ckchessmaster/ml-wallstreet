using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text;
using MLWCore.Security;

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

            // Hash the apiKey
            var dataManagerApiKey = new Password
            {
                Hash = this.config.GetValue<string>("Security:ApiKey:Hash"),
                Salt = this.config.GetValue<string>("Security:ApiKey:Salt")
            };

            Password hashedKey = SecurityHelper.HashPassword(apiKey, dataManagerApiKey.Salt);

            if (hashedKey.Equals(dataManagerApiKey))
            {
                return new JsonResult(new { Token = SecurityHelper.GenerateToken(this.config.GetValue<string>("Security:SigningKey")) });
            }

            return new UnauthorizedResult();
        }
    }
}
