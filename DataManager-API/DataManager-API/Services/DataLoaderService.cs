using DataManagerAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataManagerAPI.Services
{
    public class DataLoaderService
    {
        private readonly HttpClient client;

        public DataLoaderService(HttpClient client)
        {
            this.client = client;
        }

        public async Task<string> LoadNewData(DateTime? startDate, DateTime? endDate, string searchQuery)
        {

            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "contextualwebsearch-websearch-v1.p.rapidapi.com",
                Path = "/api/Search/NewsSearchAPI",
            };

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("autoCorrect = false");
            queryBuilder.Append("&pageNumber=1");
            queryBuilder.Append("&pageSize=10");
            queryBuilder.AppendFormat("&q={0}", string.IsNullOrEmpty(searchQuery) ? "news" : searchQuery);
            queryBuilder.Append("&safeSearch=false");

            if (startDate != null)
            {
                queryBuilder.AppendFormat("&fromPublishedDate={0}", startDate.Value.ToShortDateString());
            }

            if (endDate != null)
            {
                queryBuilder.AppendFormat("&toPublishedDate={0}", endDate.Value.ToShortDateString());
            }

            uriBuilder.Query = queryBuilder.ToString();

            using (var results = await client.GetAsync(uriBuilder.Uri))
            {
                results.EnsureSuccessStatusCode();

                return await results.Content.ReadAsStringAsync();
            }
        }
    }
}
