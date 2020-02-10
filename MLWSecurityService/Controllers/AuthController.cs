using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MLWSecurityService.Data;
using MLWSecurityService.Services;

namespace MLWSecurityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly UserService userService;
        private readonly SecurityService securityService;

        public AuthController(IConfiguration config, UserService userService, SecurityService securityService)
        {
            this.config = config;
            this.userService = userService;
            this.securityService = securityService;
        }

        public class LoginRequest
        {
            public string Username { get; set; }

            public string Password { get; set; }
        }

        public class ValidateTokenRequest
        {
            public string Token { get; set; }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody]LoginRequest request)
        {
            var user = await userService.Get(request.Username);

            if (user == null)
            {
                return new JsonResult(new { Message = "Invalid username or password." }) { StatusCode = 401 };
            }

            if (!user.IsActive)
            {
                return new JsonResult(new { Message = "User is not active." }) { StatusCode = 401 };
            }

            Password hashedPassword = securityService.HashPassword(request.Password, user.Password.Salt);

            if (hashedPassword.Equals(user.Password))
            {
                Guid refreshTokenID = Guid.NewGuid();

                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim("username", user.Username));
                
                var token = securityService.GenerateToken(identity, SecurityService.TokenType.Access);

                identity.AddClaim(new Claim("refreshTokenID", refreshTokenID.ToString()));
                var refreshToken = securityService.GenerateToken(identity, SecurityService.TokenType.Refresh);

                await userService.SetRefreshTokenID(user.Username, refreshTokenID);

                return new JsonResult(new { Token = token, RefreshToken = refreshToken });   
            }
            else
            {
                return new JsonResult(new { Message = "Invalid username or password." }) { StatusCode = 401 };
            }
        }

        [HttpGet]
        [Route("getToken")]
        public async Task<IActionResult> GetToken([FromHeader(Name = "Api-Key")]string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return new JsonResult(new { Message = "Invalid or missing api-key." }) { StatusCode = 400 };
            }

            // Hash the apiKey
            var key = new Password
            {
                Hash = config.GetValue<string>("Security:ApiKey:Hash"),
                Salt = config.GetValue<string>("Security:ApiKey:Salt")
            };

            Password hashedKey = securityService.HashPassword(apiKey, key.Salt);

            if (hashedKey.Equals(key))
            {
                var identity = new ClaimsIdentity();

                return new JsonResult(new { Token = securityService.GenerateToken(identity, SecurityService.TokenType.Access) });
            }

            return new JsonResult(new { Message = "Invalid or missing api-key." });
        }

        [HttpGet]
        [Route("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody]ValidateTokenRequest request)
        {
            // Get rid of bearer if it is still there
            string token = request.Token.Replace("Bearer ", "");

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
                ClaimsPrincipal principal = handler.ValidateToken(token, validationParams, out SecurityToken validatedToken);
                if (principal != null)
                {
                    ClaimsIdentity identity = principal.Identities.First();
                    User user = await userService.Get(identity.FindFirst("username").Value);

                    if (user.RefreshTokenID.ToString().Equals(identity.FindFirst("refreshTokenID").Value))
                    {
                        var newIdentity = new ClaimsIdentity();

                        newIdentity.AddClaim(new Claim("username", user.Username));
                        string newToken = securityService.GenerateToken(newIdentity, SecurityService.TokenType.Access);

                        Guid refreshTokenID = Guid.NewGuid();
                        newIdentity.AddClaim(new Claim("refreshTokenID", refreshTokenID.ToString()));
                        var newRefreshToken = securityService.GenerateToken(newIdentity, SecurityService.TokenType.Refresh);

                        await userService.SetRefreshTokenID(user.Username, refreshTokenID);

                        return new JsonResult(new { Token = newToken, RefreshToken = newRefreshToken });
                    } 
                    else
                    {
                        return new BadRequestResult();
                    }
                }
            }
            catch (SecurityTokenValidationException)
            {
                return new BadRequestResult();
            }
            catch (ArgumentException)
            {
                return new BadRequestResult();
            }

            return new BadRequestResult();
        }

        [HttpPost]
        [Route("validateToken")]
        public IActionResult ValidateToken([FromBody]ValidateTokenRequest request)
        {
            // Get rid of bearer if it is still there
            string token = request.Token.Replace("Bearer ", "");

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
                return new JsonResult(new { Result = false });
            }
            catch (ArgumentException)
            {
                return new BadRequestResult();
            }

            return new JsonResult(new { Result = false });
        }
    }
}
