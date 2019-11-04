using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLWSecurityService.Data
{
    public class User
    {
        [BsonIgnore]
        public static string CollectionName = "User";

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Username { get; set; }

        public Password Password { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; }
    }
}
