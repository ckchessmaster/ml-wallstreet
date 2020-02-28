using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace MLWLive.News
{
    public class ContextualWebNews : NewsStand
    {
        private readonly IHttpClientFactory clientFactory;

        private const string articleRoute = "/api/Search/NewsSearchAPI";

        public ContextualWebNews(IConfiguration config, IHttpClientFactory clientFactory) : base(config)
        {
            this.clientFactory = clientFactory;
        }

        public override async Task<IEnumerable<Article>> GetArticles()
        {
            string rapidApiHost = Config.GetValue<string>("api:contextual-web:x-rapidapi-host");
            string rapidApiKey = Config.GetValue<string>("api:contextual-web:x-rapidapi-key");

            var uriBuilder = new UriBuilder("https://" + rapidApiHost + articleRoute);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["autoCorrect"] = false.ToString();
            query["pageNumber"] = 1.ToString();
            query["pageSize"] = 50.ToString();
            query["q"] = "microsoft"; // TODO: For better results we may want to do multiple searches with different queries
            query["safeSearch"] = false.ToString();
            query["fromPublishedDate"] = DateTime.Now.AddHours(-1).ToString(); 
            query["toPublishedDate"] = DateTime.Now.ToString();
            uriBuilder.Query = query.ToString();

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            request.Headers.Add("x-rapidapi-host", rapidApiHost);
            request.Headers.Add("x-rapidapi-key", rapidApiKey);

            using HttpClient client = clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();

                var articles = new List<Article>();
                using (JsonDocument document = JsonDocument.Parse(responseJson))
                {
                    JsonElement root = document.RootElement;
                    JsonElement values = root.GetProperty("value");

                    foreach (JsonElement value in values.EnumerateArray())
                    {
                        articles.Add(new Article(
                            value.GetProperty("title").GetString(),
                            value.GetProperty("body").GetString(),
                            value.GetProperty("datePublished").GetDateTime(),
                            value.GetProperty("url").GetString()));
                    }
                }

                return articles;
            }
            else
            {
                throw new Exception("Api returned with status code: " + response.StatusCode.ToString());
            }
        }
    }
}
