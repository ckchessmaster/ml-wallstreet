using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLWLive
{
    public class Article : IMongoModel
    {
        public static string CollectionName => "Article";

        public string Id { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public DateTime DatePublished { get; set; }

        public string Url { get; set; }

        public double? Prediction { get; set; }

        public bool? ActualResult { get; set; }

        public Article(string title, string body, DateTime datePublished, string url)
        {
            Id = Guid.NewGuid().ToString();

            Title = title;
            Body = body;
            DatePublished = datePublished;
            Url = url;
        }
    }
}
