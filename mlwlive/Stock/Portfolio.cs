using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MLWLive.Stock
{
    public class Portfolio : IMongoModel
    {
        public static string CollectionName => "Portfolio";

        public string Id { get; set; }

        public decimal CurrentMoney { get; set; }

        public List<Stock> Stocks { get; set; }

        [BsonIgnore]
        public decimal CurrentWorth
        {
            get
            {
                return Stocks.Sum(s => s.Amount * s.Price);
            }
        }

        public Portfolio()
        {
            Id = Guid.NewGuid().ToString();
            Stocks = new List<Stock>();
        }
    }
}
