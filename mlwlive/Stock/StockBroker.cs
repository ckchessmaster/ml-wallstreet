using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace MLWLive.Stock
{
    public class StockBroker
    {
        private readonly IMongoCollection<Portfolio> portfolioCollection;

        // TODO: Include the ability to have multiple portfolios
        private const string portfolioId = "31c58f53-cf44-4707-ace9-c36fe0a5751e";

        // TODO: Include support for multiple stock prices
        private const string stockTicker = "MSFT";
        private const string stockName = "Microsoft";

        public StockBroker(IConfiguration config)
        {
            var client = new MongoClient(config.GetValue<string>("mongo:connection-string"));
            var database = client.GetDatabase(config.GetValue<string>("mongo:database-name"));

            portfolioCollection = database.GetCollection<Portfolio>(Portfolio.CollectionName);
        }

        public async Task CreatePortfolio()
        {
            var portfolio = new Portfolio()
            {
                Stocks = new List<Stock>()
            };

            await portfolioCollection.InsertOneAsync(portfolio);
        }

        public async Task AddFunds(decimal amount)
        {
            var filter = Builders<Portfolio>.Filter
                .Eq("_id", portfolioId);

            var portfolio = (await portfolioCollection.FindAsync<Portfolio>(filter)).First(); // For now there should always be one and only one
            portfolio.CurrentMoney += amount;

            var update = Builders<Portfolio>.Update
                .Set("CurrentMoney", portfolio.CurrentMoney);

            await portfolioCollection.UpdateOneAsync(filter, update);
        }

        public async Task HandlePrediction(Article article)
        {
            if (article == null)
            {
                throw new ArgumentNullException("Parameter article cannot be null.");
            }

            if (article.Prediction > 0.7)
            {
                await BuyStock();
            } 
            else if (article.Prediction < 0.3)
            {
                await SellStock();
            }
        }

        public async Task BuyStock()
        {
            // Get Current Price
            decimal stockPrice = await GetCurrentStockPrice();

            // Buy
            var filter = Builders<Portfolio>.Filter
                .Eq("_id", portfolioId);

            var portfolio = (await portfolioCollection.FindAsync<Portfolio>(filter)).First();

            // TODO: Handle this properly
            if (portfolio.CurrentMoney < stockPrice)
            {
                throw new Exception("Not enough money!");
            }

            decimal newMoney = portfolio.CurrentMoney - stockPrice;

            Stock stock = portfolio.Stocks.Where(s => s.Ticker.Equals(stockTicker)).FirstOrDefault();

            if (stock is null)
            {
                stock = new Stock
                {
                    Name = stockName,
                    Ticker = stockTicker,
                    Amount = 1,
                    Cost = stockPrice
                };

                portfolio.Stocks.Add(stock);
            }
            else
            {
                stock.Amount += 1;
            }

            var update = Builders<Portfolio>.Update
                .Set("CurrentMoney", newMoney)
                .Set("Stocks", portfolio.Stocks);

            await portfolioCollection.UpdateOneAsync(filter, update);
        }

        public async Task SellStock()
        {
            // Get Current Price
            decimal stockPrice = await GetCurrentStockPrice();

            // Sell
            var filter = Builders<Portfolio>.Filter
                .Eq("_id", portfolioId);

            var portfolio = (await portfolioCollection.FindAsync<Portfolio>(filter)).First();

            Stock stock = portfolio.Stocks.Where(s => s.Ticker.Equals(stockTicker)).FirstOrDefault();

            if (stock is null)
            {
                return; // We don't own any of this stock so just return
            }

            decimal newMoney = portfolio.CurrentMoney + stockPrice;

            stock.Amount -= 1;

            if (stock.Amount == 0)
            {
                portfolio.Stocks.Remove(stock);
            }

            var update = Builders<Portfolio>.Update
                .Set("CurrentMoney", newMoney)
                .Set("Stocks", portfolio.Stocks);

            await portfolioCollection.UpdateOneAsync(filter, update);
        }

        private async Task<decimal> GetCurrentStockPrice()
        {
            return 100.0M;
        }

        // TODO: May not need this...
        public async Task EvaluateResults()
        { 

        }
    }
}
