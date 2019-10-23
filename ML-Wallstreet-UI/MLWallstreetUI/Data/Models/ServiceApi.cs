using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;

namespace MLWallstreetUI.Data.Models
{
    public class ServiceApi
    {
        private string apiToken;

        public string BaseUrl { get; private set; }

        public HttpClient Client { get; set; }

        private readonly IConfiguration configuration;

        public ServiceApi(IConfiguration configuration, string baseUrl)
        {
            this.configuration = configuration;

            BaseUrl = baseUrl;

            Client = new HttpClient();
        }

        public bool IsAuthenticated()
        {
            var jwtHandler = new JwtSecurityTokenHandler();

            if (string.IsNullOrEmpty(apiToken) || jwtHandler.ReadJwtToken(apiToken).ValidTo < DateTime.UtcNow.AddMinutes(1))
            {
                return false;
            }

            return true;
        }

        public async Task Authenticate()
        {
            // Get the new token
            Client.DefaultRequestHeaders.Add("Api-Key", configuration.GetValue<string>("SecurityServiceAPI:ApiKey"));
            var result = await Client.GetAsync(configuration.GetValue<string>("SecurityServiceAPI:Url") + "auth/gettoken");

            result.EnsureSuccessStatusCode();

            string jsonString = await result.Content.ReadAsStringAsync();
            var authResult = (JObject)JsonConvert.DeserializeObject(jsonString);

            apiToken = authResult["token"].Value<string>();
            Client.DefaultRequestHeaders.Remove("Api-Key"); // Remove the request header with the api key

            // Add the token as our authorization
            Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiToken);
        }
    }
}
