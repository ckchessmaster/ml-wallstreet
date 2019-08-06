using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MLWCore.Security
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash the given password using an optional salt. (If no salt is provided one will be generated)
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt to use.</param>
        /// <returns>A Password object containing the hashed password and it's salt.</returns>
        public static Password HashPassword(string password, byte[] salt = null)
        {
            // generate a 128-bit salt using a secure PRNG
            if (salt == null)
            {
                salt = new byte[128 / 8];
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return new Password
            {
                Hash = hashed,
                Salt = Encoding.ASCII.GetString(salt)
            };
        }
    }
}
