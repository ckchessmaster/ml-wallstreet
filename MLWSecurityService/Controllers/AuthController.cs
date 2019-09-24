using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MLWCore.Security;

namespace MLWSecurityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
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
                Hash = config.GetValue<string>("Security:ApiKey:Hash"),
                Salt = config.GetValue<string>("Security:ApiKey:Salt")
            };

            Password hashedKey = SecurityHelper.HashPassword(apiKey, dataManagerApiKey.Salt);

            if (hashedKey.Equals(dataManagerApiKey))
            {
                return new JsonResult(new { Token = SecurityHelper.GenerateToken(
                    config.GetValue<string>("Security:SigningKey"), 
                    config.GetValue<string>("Security:Issuer"), 
                    config.GetValue<string>("Security:Audience")) });
            }

            return new UnauthorizedResult();
        }

        [HttpPost]
        [Route("validateToken")]
        public IActionResult ValidateToken([FromBody]string token)
        {
            // Get rid of bearer if it is still there
            token = token.Replace("Bearer ", "");

            var validationParams = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.GetValue<string>("Security:SigningKey"))),
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = config.GetValue<string>("Security:Issuer"),
                ValidateAudience = true,
                ValidAudience = config.GetValue<string>("Security:Audience"),
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            try
            {
                var user = handler.ValidateToken(token, validationParams, out SecurityToken validatedToken);
                if (user != null)
                {
                    return new JsonResult(new { Result = true });
                }

            }
            catch (SecurityTokenValidationException)
            {
                return new UnauthorizedResult();
            }
            catch (ArgumentException)
            {
                return new BadRequestResult();
            }

            return new JsonResult(new { Result = false });
        }
    }
}
