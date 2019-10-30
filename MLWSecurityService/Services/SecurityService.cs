using Microsoft.AspNetCore.Cryptography.KeyDerivation;
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
        /// <summary>
        /// Hash the given password using an optional salt. (If no salt is provided one will be generated)
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt to use.</param>
        /// <returns>A Password object containing the hashed password and it's salt.</returns>
        public static Password HashPassword(string password, string salt = null)
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

        public static string GenerateToken(string signingKey, string Issuer, string Audience)
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
                Audience = Audience,
                Issuer = Issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
