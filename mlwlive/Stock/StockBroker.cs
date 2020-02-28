using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Text.Json;

namespace MLWLive.Stock
{
    public class StockBroker
    {
        private readonly IMongoCollection<Portfolio> portfolioCollection;
        private readonly IConfiguration config;
        private readonly IHttpClientFactory clientFactory;

        // TODO: Include the ability to have multiple portfolios
        private const string portfolioId = "31c58f53-cf44-4707-ace9-c36fe0a5751e";

        // TODO: Include support for multiple stock prices
        private const string stockTicker = "MSFT";
        private const string stockName = "Microsoft";

        // Make the last retrieval a day behind so that the first hit will go ahead and get the current price
        private DateTime lastPriceRetrieval = DateTime.Now.AddDays(-1);
        private decimal currentStockPrice = 0.0M;

        public StockBroker(IConfiguration config, IHttpClientFactory clientFactory)
        {
            this.config = config;
            this.clientFactory = clientFactory;

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

        public async Task HandlePredictions(IEnumerable<Article> articles)
        {
            if (articles == null)
            {
                throw new ArgumentNullException("Parameter articles cannot be null.");
            }

            int decision = 0;
            foreach (Article article in articles)
            {
                if (article.Prediction > 0.7)
                {
                    decision++;
                }
                else if (article.Prediction < 0.3)
                {
                    decision--;
                }
            }

            if (decision >= 1)
            {
                await BuyStock();

            }
            else if (decision <= -1)
            {
                await SellStock();
            }
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
            var filter = Builders<Portfolio>.Filter
                .Eq("_id", portfolioId);

            var portfolio = (await portfolioCollection.FindAsync<Portfolio>(filter)).First();
            Stock stock = portfolio.Stocks.Where(s => s.Ticker.Equals(stockTicker)).FirstOrDefault();

            bool isNewStock = false;
            if (stock is null)
            {
                stock = new Stock
                {
                    Name = stockName,
                    Ticker = stockTicker,
                    Amount = 1,
                    LastPriceCheck = DateTime.Now.AddDays(-1)
                };

                isNewStock = true;
            }

            stock.Price = await GetCurrentStockPrice(stock);

            // TODO: Handle this properly
            if (portfolio.CurrentMoney < stock.Price)
            {
                throw new Exception("Not enough money!");
            }

            // Buy
            decimal newMoney = portfolio.CurrentMoney - stock.Price;
            
            if (isNewStock)
            {
                portfolio.Stocks.Add(stock);
            }
            else
            {
                stock.Amount++;
            }

            var update = Builders<Portfolio>.Update
                .Set("CurrentMoney", newMoney)
                .Set("Stocks", portfolio.Stocks);

            await portfolioCollection.UpdateOneAsync(filter, update);
        }

        public async Task SellStock()
        {
            // Sell
            var filter = Builders<Portfolio>.Filter
                .Eq("_id", portfolioId);

            var portfolio = (await portfolioCollection.FindAsync<Portfolio>(filter)).First();
            Stock stock = portfolio.Stocks.Where(s => s.Ticker.Equals(stockTicker)).FirstOrDefault();

            if (stock is null)
            {
                return; // We don't own any of this stock so just return
            }

            decimal newMoney = portfolio.CurrentMoney + await GetCurrentStockPrice(stock);

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

        public async Task<decimal> GetCurrentStockPrice(Stock stock)
        {
            // We only want to check for a new price every half hour
            if (stock.LastPriceCheck > DateTime.Now.AddMinutes(-30))
            {
                return stock.Price;
            }

            string rapidApiHost = config.GetValue<string>("api:alpha-vantage:x-rapidapi-host");
            string rapidApiKey = config.GetValue<string>("api:alpha-vantage:x-rapidapi-key");

            var uriBuilder = new UriBuilder("https://" + rapidApiHost + "/query");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["function"] = "GLOBAL_QUOTE";
            query["symbol"] = stock.Ticker.ToUpper();
            query["datatype"] = "json";
            uriBuilder.Query = query.ToString();

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            request.Headers.Add("x-rapidapi-host", rapidApiHost);
            request.Headers.Add("x-rapidapi-key", rapidApiKey);

            using HttpClient client = clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();

                using JsonDocument document = JsonDocument.Parse(responseJson);
                JsonElement root = document.RootElement;
                JsonElement quote = root.GetProperty("Global Quote");
                JsonElement price = quote.GetProperty("05. price");

                return Convert.ToDecimal(price.GetString());
            }
            else
            {
                throw new Exception("Api returned with status code: " + response.StatusCode.ToString());
            }
        }

        // TODO: May not need this...
        public async Task EvaluateResults()
        { 

        }
    }
}
