using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MLWLive.News;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MLWLive
{
    public class Manager
    {
        private readonly ServiceProvider serviceProvider;

        private readonly IEnumerable<NewsStand> newsStands;
        private List<Timer> newsTimers;

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
            // Retrieve new articles
            var articles = await newsStand.GetArticles();

            // Predict results
            var prohpet = serviceProvider.GetService<Prophet>();
            articles = await prohpet.Predict(articles);

            // Save articles
            await newsStand.SaveArticles(articles);


        }
    }
}
