using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MLWLive.News;
using MLWLive.Stock;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;

namespace MLWLive
{
    public class Manager
    {
        private readonly ServiceProvider serviceProvider;

        private readonly IEnumerable<NewsStand> newsStands;
        private List<Timer> newsTimers = new List<Timer>();

        private static readonly DayOfWeek[] weekendDays = { DayOfWeek.Saturday, DayOfWeek.Sunday };
        private static DateTime lastArticleRetrieval; // TODO: Probably a better way to handle this

        public Manager(ServiceProvider serviceProvider, IEnumerable<NewsStand> newsStands)
        {
            this.serviceProvider = serviceProvider;
            this.newsStands = newsStands;
        }

        public void Start()
        {
            int predictionInterval = 3600000; // 1 hours in milliseconds

            foreach(NewsStand newsStand in newsStands)
            {
                var timer = new Timer(predictionInterval)
                {
                    AutoReset = true
                };

                timer.Elapsed += (sender, e) => Tasks(sender, e, newsStand, serviceProvider);
                timer.Start();

                newsTimers.Add(timer);
            }

            Console.WriteLine("Program running. Press enter to exit...");
            Console.ReadLine();
        }

        private static async Task Tasks(object sender, ElapsedEventArgs e, NewsStand newsStand, ServiceProvider serviceProvider)
        {
            // We can only buy/sell during business hours/work days
            DateTime currentTime = DateTime.Now;
            if (weekendDays.Contains(currentTime.DayOfWeek) || currentTime.Hour < 9 || currentTime.Hour >= 17)
            {
                Console.WriteLine($"Not business hours. Trying again in an hour. Current time is: {DateTime.Now}");
                return;
            }

            // Retrieve new articles
            Console.WriteLine("Retrieving new articles...");
            var articles = await newsStand.GetArticles(lastArticleRetrieval);
            lastArticleRetrieval = DateTime.Now;
            Console.WriteLine($"Retrieved {articles.Count()} articles.");

            // Predict results
            Console.WriteLine("Predicting results...");
            var prohpet = serviceProvider.GetService<Prophet>();
            articles = await prohpet.Predict(articles);

            // Save articles
            Console.WriteLine("Saving articles...");
            await newsStand.SaveArticles(articles);

            // Lets try our luck!
            Console.WriteLine("Attempting to buy/sell stock...");
            var stockBroker = serviceProvider.GetService<StockBroker>();
            await stockBroker.HandlePredictions(articles);

            Console.WriteLine($"Process complete. Current time is: {DateTime.Now}");
        }
    }
}
