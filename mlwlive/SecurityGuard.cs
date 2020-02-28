using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MLWLive
{
    public class SecurityGuard
    {
        private readonly IConfiguration config;
        private readonly IHttpClientFactory clientFactory;

        private string? accessToken;

        public SecurityGuard(IConfiguration config, IHttpClientFactory clientFactory)
        {
            this.config = config;
            this.clientFactory = clientFactory;
        }

        public async Task<string> GetAccessToken()
        {
            if (!string.IsNullOrEmpty(accessToken) && ValidateToken(accessToken))
            {
                return accessToken;
            }

            accessToken = await GetNewToken().ConfigureAwait(false);

            return accessToken;
        }

        private bool ValidateToken(string token)
        {
            var validatorParams = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = false
            };

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(token, validatorParams, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception e)
            {
                if (e is SecurityTokenExpiredException)
                {
                    return false;
                }

                throw;
            }
        }

        private async Task<string> GetNewToken()
        {
            string apiKey = config.GetValue<string>("api:ml-security-service:api-key");
            string url = config.GetValue<string>("api:ml-security-service:url") + "auth/gettoken";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Api-Key", apiKey);

            using HttpClient client = clientFactory.CreateClient();
            var response = await client.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                using JsonDocument document = JsonDocument.Parse(responseJson);
                JsonElement root = document.RootElement;

                return root.GetProperty("token").GetString();
            }
            else
            {
                throw new Exception("Api returned with status code: " + response.StatusCode.ToString());
            }
        }
    }
}
