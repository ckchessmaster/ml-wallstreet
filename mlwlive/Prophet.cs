using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MLWLive
{
    public class Prophet
    {
        private readonly IConfiguration config;
        private readonly IHttpClientFactory clientFactory;
        private readonly SecurityGuard securityGuard;

        public Prophet(IConfiguration config, IHttpClientFactory clientFactory, SecurityGuard securityGuard)
        {
            this.config = config;
            this.clientFactory = clientFactory;
            this.securityGuard = securityGuard;
        }

        public async Task<IEnumerable<Article>> Predict(IEnumerable<Article> articles)
        {
            string url = config.GetValue<string>("api:ml-service:url") + "models/" + config.GetValue<string>("api:ml-service:model-type") + "/predict";

            var articleTexts = articles.Select(a => a.Body);

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await securityGuard.GetAccessToken());

            using HttpClient client = clientFactory.CreateClient();

            request.Content = new StringContent(JsonSerializer.Serialize(new { texts = articleTexts }), Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();

                using JsonDocument document = JsonDocument.Parse(responseJson);
                JsonElement root = document.RootElement;
                List<JsonElement> results = root.GetProperty("results").EnumerateArray().ToList();

                var articleList = articles.ToList();
                for (int i = 0; i < articles.Count(); i++)
                {
                    articleList[i].Prediction = results[i].EnumerateArray().First().GetDouble();
                }

                return articleList;
            }
            else
            {
                throw new Exception("Api returned with status code: " + response.StatusCode.ToString());
            }
        }
    }
}
