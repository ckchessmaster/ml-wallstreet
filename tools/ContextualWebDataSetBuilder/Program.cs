using CsvHelper;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;

namespace ContextualWebDataSetBuilder
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            Console.Write("Search term: ");
            string searchText = Console.ReadLine();

            Console.Write("StartDate: ");
            DateTime startDate = Convert.ToDateTime(Console.ReadLine());

            Console.Write("EndDate: ");
            DateTime endDate = Convert.ToDateTime(Console.ReadLine());

            Console.Write("# of Results: ");
            int numResults = Convert.ToInt32(Console.ReadLine());

            int currentPage = 1;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-rapidapi-host", "contextualwebsearch-websearch-v1.p.rapidapi.com");
            client.DefaultRequestHeaders.Add("x-rapidapi-key", config["RapidAPIKey"]);

            string url = $"https://contextualwebsearch-websearch-v1.p.rapidapi.com/api/Search/NewsSearchAPI" +
                $"?autoCorrect=false" +
                $"&pageNumber={currentPage.ToString()}" +
                $"&pageSize={numResults.ToString()}" +
                $"&q={HttpUtility.UrlEncode(searchText)}" +
                $"&safeSearch=false" +
                $"&fromPublishedDate={HttpUtility.UrlEncode(startDate.ToString())}" +
                $"&toPublishedDate={HttpUtility.UrlEncode(endDate.ToString())}";

            Console.WriteLine("Retrieving page 1");
            var response = await client.GetAsync(url);
            string responseString = await response.Content.ReadAsStringAsync();
            NewsResult finalResult = JsonSerializer.Deserialize<NewsResult>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            double numPages = Math.Ceiling((float)finalResult.TotalCount / (float)numResults);

            if (numPages >= 500)
            {
                Console.WriteLine("WARNING! The number of calls about to be made will be greater than 500. Type Y to continue.");
                bool shouldContinue = Console.ReadLine().ToUpper().Equals("Y");

                if (!shouldContinue)
                {
                    End();
                    return;
                }
            }

            while (currentPage * numResults < finalResult.TotalCount)
            {
                currentPage++;

                url = $"https://contextualwebsearch-websearch-v1.p.rapidapi.com/api/Search/NewsSearchAPI" +
                    $"?autoCorrect=false" +
                    $"&pageNumber={currentPage.ToString()}" +
                    $"&pageSize={numResults.ToString()}" +
                    $"&q={HttpUtility.UrlEncode(searchText)}" +
                    $"&safeSearch=false" +
                    $"&fromPublishedDate={HttpUtility.UrlEncode(startDate.ToString())}" +
                    $"&toPublishedDate={HttpUtility.UrlEncode(endDate.ToString())}";

                Console.WriteLine($"Retrieving page {currentPage.ToString()} / {numPages}");
                response = await client.GetAsync(url);
                responseString = await response.Content.ReadAsStringAsync();
                NewsResult nextResult = JsonSerializer.Deserialize<NewsResult>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                finalResult.Value.AddRange(nextResult.Value);

                if (nextResult.Value.Count < 50)
                {
                    break;
                }
            }

            client.Dispose();

            using (var writer = new StreamWriter(Directory.GetCurrentDirectory() + "\\NewsData.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(finalResult.Value);
            }

            Console.WriteLine($"Done!\nTotalResults: {finalResult.TotalCount}\nResultsSaved:{finalResult.Value.Count}");
            End();
        }

        private static void End()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
