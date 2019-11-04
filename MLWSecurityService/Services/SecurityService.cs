using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MLWSecurityService.Data;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MLWSecurityService.Services
{
    public class SecurityService
    {
        private readonly string signingKey;
        private readonly string issuer;
        private readonly string audience;

        public SecurityService(IConfiguration config)
        {
            signingKey = config.GetValue<string>("Security:SigningKey");
            issuer = config.GetValue<string>("Security:Issuer");
            audience = config.GetValue<string>("Security:Audience");
        }

        /// <summary>
        /// Hash the given password using an optional salt. (If no salt is provided one will be generated)
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt to use.</param>
        /// <returns>A Password object containing the hashed password and it's salt.</returns>
        public Password HashPassword(string password, string salt = null)
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] saltBytes;

            if (salt == null)
            {
                saltBytes = new byte[128 / 8];

                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }
            }
            else
            {
                saltBytes = Convert.FromBase64String(salt);
            }

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return new Password
            {
                Hash = hashed,
                Salt = Convert.ToBase64String(saltBytes)
            };
        }

        public string GenerateToken()
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(signingKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(ClaimTypes.Name, "DataManagerAccessToken")
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
