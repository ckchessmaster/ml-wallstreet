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
    //public class BloomburgFinancialNews : INewsStand
    //{
    //    private readonly IConfiguration config;
    //    private readonly IHttpClientFactory clientFactory;

    //    private const string articleRoute = "/stories/list";

    //    public int DeliveryInterval => 24;

    //    public BloomburgFinancialNews(IConfiguration config, IHttpClientFactory clientFactory)
    //    {
    //        this.config = config;
    //        this.clientFactory = clientFactory;
    //    }

    //    public async Task<IEnumerable<Article>> GetArticles()
    //    {
    //        throw new NotImplementedException("Need a screen scraper or bloomburg subscription to normalize articles.");

    //        string rapidApiHost = config.GetValue<string>("api:bloomburg-financial-news:x-rapidapi-host");
    //        string rapidApiKey = config.GetValue<string>("api:bloomburg-financial-news:x-rapidapi-key");
    //        string template = config.GetValue<string>("api:bloomburg-financial-news:template");
    //        string id = config.GetValue<string>("api:bloomburg-financial-news:id");

    //        var uriBuilder = new UriBuilder("https://" + rapidApiHost + articleRoute);
    //        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
    //        query["template"] = template;
    //        query["id"] = HttpUtility.UrlEncode(id);
    //        uriBuilder.Query = query.ToString();

    //        using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());  
    //        request.Headers.Add("x-rapidapi-host", rapidApiHost);
    //        request.Headers.Add("x-rapidapi-key", rapidApiKey);

    //        using HttpClient client = clientFactory.CreateClient();
    //        var response = await client.SendAsync(request).ConfigureAwait(false);

    //        if (response.IsSuccessStatusCode)
    //        {
    //            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

    //            // TODO: Use JsonDocument instead
    //            var bloomburgFinancialNewsArticles = JsonSerializer.Deserialize<List<BloomburgFinancialNewsArticle>>(responseJson, new JsonSerializerOptions());

    //            var articleNormalizers = new List<Task<Article>>();
    //            foreach (var article in bloomburgFinancialNewsArticles)
    //            {
    //                articleNormalizers.Add(NormalizeArticle(article));
    //            }

    //            return await Task.WhenAll(articleNormalizers.ToArray()).ConfigureAwait(false);
    //        }
    //        else
    //        {
    //            throw new Exception("Api returned with status code: " + response.StatusCode.ToString());
    //        }
    //    }

    //    private async Task<Article> NormalizeArticle(BloomburgFinancialNewsArticle article)
    //    {
    //        DateTime publishDate = DateTimeOffset.FromUnixTimeSeconds(article.Published).UtcDateTime;

    //        using HttpClient client = clientFactory.CreateClient();


    //        return new Article(article.Title, "", publishDate, article.Url);
    //    }

    //    private class BloomburgFinancialNewsArticle
    //    {
    //        [JsonPropertyName("title")]
    //        public string Title { get; set; }

    //        [JsonPropertyName("id")]
    //        public string Id { get; set; }

    //        [JsonPropertyName("published")]
    //        public int Published { get; set; }

    //        [JsonPropertyName("longURL")]
    //        public string Url { get; set; }
    //    }
    //}
}
