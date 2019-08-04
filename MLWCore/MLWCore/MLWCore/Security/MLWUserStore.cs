using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MLWCore.Security
{
    public class MLWUserStore : 
        IUserStore<MLWUser>, 
        IUserEmailStore<MLWUser>, 
        IUserPasswordStore<MLWUser>
    {
        private readonly IConfiguration config;

        public MLWUserStore(IConfiguration config)
        {
            this.config = config;
        }

        public async Task<IdentityResult> CreateAsync(MLWUser user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException();

            string sql = @"
                INSERT INTO MLWUser (MLWUserID, UserName, Email, Password, IsActive) 
                VALUES (@MLWUserID, @UserName, @Email, @Password, @IsActive)";

            using (var con = GetSqlConnection())
            {
                await con.ExecuteAsync(sql, new {
                    MLWUserID = user.Id,
                    UserName = string.IsNullOrEmpty(user.UserName) ? user.Email : user.UserName,
                    Email = user.Email,
                    Password = user.PasswordHash,
                    IsActive = false });
            }

            return new IdentityResult();
        }

        public async Task<IdentityResult> DeleteAsync(MLWUser user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException();

            string sql = "DELETE FROM MLWUser WHERE MLWUserID = @UserID";

            using (var con = GetSqlConnection())
            {
                await con.ExecuteAsync(sql, new { UserID = user.Id });
            }

            return new IdentityResult();
        }

        public void Dispose()
        {
            // We don't actually need to do this. (Not yet anyway)
        }

        public async Task<MLWUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(normalizedEmail))
                throw new ArgumentNullException();

            string sql = "SELECT * FROM MLWUser WHERE Email LIKE @Email";

            using (var con = GetSqlConnection())
            {
                return await con.QueryFirstOrDefaultAsync<MLWUser>(sql, new { Email = normalizedEmail });
            }
        }

        public async Task<MLWUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException();

            string sql = "SELECT * FROM MLWUser WHERE MLWUserID = @MLWUserID";

            using (var con = GetSqlConnection())
            {
                var result = await con.QueryFirstOrDefaultAsync<MLWUser>(sql, new { MLWUserID = userId });

                return result;
            }
        }

        public async Task<MLWUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(normalizedUserName))
                throw new ArgumentNullException();

            string sql = "SELECT * FROM MLWUser WHERE UserName = @UserName";

            using (var con = GetSqlConnection())
            {
                var result = await con.QueryFirstOrDefaultAsync<MLWUser>(sql, new { UserName = normalizedUserName });

                return result;
            }
        }

        public async Task<string> GetEmailAsync(MLWUser user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException();

            if (!string.IsNullOrEmpty(user.Email))
            {
                return user.Email;
            }

            string sql = "SELECT Email FROM MLWUser WHERE MLWUserID = @MLWUserID";

            using (var con = GetSqlConnection())
            {
                return await con.QueryFirstOrDefaultAsync<string>(sql, new { MLWUserID = user.Id });
            }
        }

        public async Task<bool> GetEmailConfirmedAsync(MLWUser user, CancellationToken cancellationToken)
        {
            return true;
        }

        public async Task<string> GetNormalizedEmailAsync(MLWUser user, CancellationToken cancellationToken)
        {
            return (await GetEmailAsync(user, cancellationToken)).ToUpper();
        }

        public async Task<string> GetNormalizedUserNameAsync(MLWUser user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException();

            return (await GetUserNameAsync(user, cancellationToken)).ToUpper();
        }

        public async Task<string> GetUserIdAsync(MLWUser user, CancellationToken cancellationToken)
        {
            if (user == null || string.IsNullOrEmpty(user.Id))
                throw new ArgumentNullException();
            
            return user.Id;
        }

        public async Task<string> GetUserNameAsync(MLWUser user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException();

            if (!string.IsNullOrEmpty(user.UserName))
                return user.UserName;

            string sql = "SELECT UserName FROM MLWUser WHERE MLWUserID = @MLWUserID";

            using (var con = GetSqlConnection())
            {
                var result = await con.QueryFirstOrDefaultAsync<string>(sql, new { MLWUserID = user.Id });

                return result;
            }
        }

        public async Task<string> GetPasswordHashAsync(MLWUser user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException();

            if (!string.IsNullOrEmpty(user.PasswordHash))
                return user.PasswordHash;

            string sql = "SELECT Password FROM MLWUser WHERE MLWUserID = @MLWUserID OR UserName LIKE @UserName";

            using (var con = GetSqlConnection())
            {
                var result = await con.QueryFirstOrDefaultAsync<string>(sql, new { MLWUserID = user.Id, UserName = user.UserName });
                return result;
            }
        }

        public async Task<bool> HasPasswordAsync(MLWUser user, CancellationToken cancellationToken)
        {
            // Every user MUST have a password
            return true;
        }

        public async Task SetPasswordHashAsync(MLWUser user, string passwordHash, CancellationToken cancellationToken)
        {
            if (user == null || string.IsNullOrEmpty(passwordHash))
                throw new ArgumentNullException();

            string sql = "UPDATE MLWUser SET Password = @Password";

            using (var con = GetSqlConnection())
            {
                await con.ExecuteAsync(sql, new { MLWUserID = user.Id, Password = passwordHash });
            }

            user.PasswordHash = passwordHash;
        }

        public async Task SetEmailAsync(MLWUser user, string email, CancellationToken cancellationToken)
        {
            if (user == null || string.IsNullOrEmpty(email))
                throw new ArgumentNullException();

            string sql = "UPDATE MLWUser SET Email = @Email WHERE MLWUserID = @MLWUserID";

            using (var con = GetSqlConnection())
            {
                await con.ExecuteAsync(sql, new { MLWUserID = user.Id, Email = email });
            }

            user.Email = email;
        }

        public async Task SetEmailConfirmedAsync(MLWUser user, bool confirmed, CancellationToken cancellationToken)
        {
            // Do nothing
        }

        public async Task SetNormalizedEmailAsync(MLWUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            // Do nothing
        }

        public async Task SetNormalizedUserNameAsync(MLWUser user, string normalizedName, CancellationToken cancellationToken)
        {
            // For now do nothing
        }

        public async Task SetUserNameAsync(MLWUser user, string userName, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException();

            string sql = "UPDATE MLWUser SET UserName = @UserName WHERE MLWUserID = @MLWUserID";

            using (var con = GetSqlConnection())
            {
                await con.ExecuteAsync(sql, new { MLWUserID = user.Id, UserName = userName });
            }

            user.UserName = userName;
        }

        public async Task<IdentityResult> UpdateAsync(MLWUser user, CancellationToken cancellationToken)
        {
            // For now we can't update users
            return new IdentityResult();
        }

        private SqlConnection GetSqlConnection()
        {
            return new SqlConnection(config.GetConnectionString("MLWCoreConnectionString"));
        }
    }
}
