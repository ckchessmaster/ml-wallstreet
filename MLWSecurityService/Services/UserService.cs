using Microsoft.Extensions.Configuration;
using MLWSecurityService.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MLWSecurityService.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> users;
        private readonly SecurityService securityService;

        public UserService(IConfiguration config, SecurityService securityService)
        {
            var client = new MongoClient(config.GetValue<string>("Mongo:ConnectionString"));
            var database = client.GetDatabase(config.GetValue<string>("Mongo:DatabaseName"));

            users = database.GetCollection<User>(User.CollectionName);

            this.securityService = securityService;
        }

        public async Task<User> Get(string username)
        {
            return (await users.FindAsync<User>(user => user.Username == username)).FirstOrDefault();
        }

        public async Task Create(string username, string password, string email)
        {
            Password hashedPassword = securityService.HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Password = hashedPassword,
                Email = email,
                IsActive = false,
            };

            await users.InsertOneAsync(newUser, new InsertOneOptions());
        }

        public async Task SetRefreshTokenID(string username, Guid refreshTokenID)
        {
            var filter = Builders<User>.Filter
                .Eq("Username", username);

            var update = Builders<User>.Update
                .Set("RefreshTokenID", refreshTokenID);

            await users.UpdateOneAsync(filter, update);
        }
    }
}
