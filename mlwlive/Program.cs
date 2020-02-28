using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MLWLive.News;
using MLWLive.Stock;

namespace MLWLive
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = SetupDI();

            var test = serviceProvider.GetService<StockBroker>();
            //await test.AddFunds(1000000M);
            //await test.BuyStock();
            // await test.SellStock();

            var asdf = await test.GetCurrentStockPrice();
            Console.WriteLine();
            //var manager = new Manager(serviceProvider, new List<NewsStand>() { serviceProvider.GetService<ContextualWebNews>() });

            //manager.Start();
        }

        private static ServiceProvider SetupDI()
        {
            // Config setup
            IConfiguration config = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", true, true)
              .AddUserSecrets<Program>()
              .Build();

            // Setup Dependency Injection
            var serviceProvider = new ServiceCollection()
                .AddSingleton(config)
                .AddHttpClient()
                .AddSingleton<SecurityGuard>()
                .AddSingleton<ContextualWebNews>()
                .AddSingleton<Prophet>()
                .AddSingleton<StockBroker>()
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
