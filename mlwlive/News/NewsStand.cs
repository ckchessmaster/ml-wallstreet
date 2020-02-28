using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MLWLive.News
{
    public abstract class NewsStand
    {
        private readonly IMongoCollection<Article> articleCollection;

        protected IConfiguration Config { get; private set; }

        protected NewsStand(IConfiguration config)
        {
            Config = config;

            var client = new MongoClient(config.GetValue<string>("mongo:connection-string"));
            var database = client.GetDatabase(config.GetValue<string>("mongo:database-name"));
            
            articleCollection = database.GetCollection<Article>(Article.CollectionName);
        }

        public abstract Task<IEnumerable<Article>> GetArticles(DateTime fromPublishedDate);

        public virtual async Task SaveArticles(IEnumerable<Article> articles)
        {
            await articleCollection.InsertManyAsync(articles);
        }
    }
}
