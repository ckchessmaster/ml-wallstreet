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
    //public class NewsAPI : INewsStand
    //{
    //    private readonly IConfiguration config;
    //    private readonly IHttpClientFactory clientFactory;

    //    private const string articleRoute = "newsapi.org/v2/top-headlines";

    //    public int DeliveryInterval => 1;

    //    public NewsAPI(IConfiguration config, IHttpClientFactory clientFactory)
    //    {
    //        this.config = config;
    //        this.clientFactory = clientFactory;
    //    }

    //    public async Task<IEnumerable<Article>> GetArticles()
    //    {
    //        throw new NotImplementedException("The news api truncates articles at 260 chars for the free developer tier.");

    //        string newsApiApiKey = config.GetValue<string>("api:news-api:api-key");

    //        var uriBuilder = new UriBuilder("https://" + articleRoute);
    //        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
    //        query["country"] = "us";
    //        query["apiKey"] = newsApiApiKey;
    //        query["pageSize"] = 100.ToString();
    //        uriBuilder.Query = query.ToString();

    //        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
    //        using HttpClient client = clientFactory.CreateClient();
    //        var response = await client.SendAsync(request).ConfigureAwait(false);

    //        if (response.IsSuccessStatusCode)
    //        {
    //            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

    //            // TODO: Use JsonDocument instead
    //            var newsApiNewsArticles = JsonSerializer.Deserialize<List<NewsApiNewsArticle>>(responseJson, new JsonSerializerOptions());

    //            var normalizedArtciles = new List<Article>();
    //            foreach (var article in newsApiNewsArticles)
    //            {
    //                normalizedArtciles.Add(NormalizeArticle(article));
    //            }

    //            return normalizedArtciles;
    //        }
    //        else
    //        {
    //            throw new Exception("Api returned with status code: " + response.StatusCode.ToString());
    //        }
    //    }

    //    private Article NormalizeArticle(NewsApiNewsArticle article)
    //    {
    //        return new Article(article.Title, article.Content, article.PublishedAt, article.Url);
    //    }

    //    private class NewsApiNewsArticle
    //    {
    //        [JsonPropertyName("title")]
    //        public string Title { get; set; }

    //        [JsonPropertyName("publishedAt")]
    //        public DateTime PublishedAt { get; set; }

    //        [JsonPropertyName("content")]
    //        public string Content { get; set; }

    //        [JsonPropertyName("url")]
    //        public string Url { get; set; }
    //    }
    //}
}
