using Dapper;
using DataManagerAPI.Enums;
using DataManagerAPI.Models.ServiceModels.ContextualWebSearch;
using DataManagerAPI.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataManagerAPI.Services
{
    public class DataLoaderService
    {
        private readonly HttpClient client;

        private readonly SqlHelper sqlHelper;

        private readonly IConfiguration config;

        public DataLoaderService(
            HttpClient client,
            SqlHelper sqlHelper,
            IConfiguration config)
        {
            this.client = client;
            this.sqlHelper = sqlHelper;
            this.config = config;
        }

        public async Task<ContextualWebSearchResult> LoadNewData(DateTime? startDate, DateTime? endDate, string searchQuery, int? pageSize)
        {
            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "contextualwebsearch-websearch-v1.p.rapidapi.com",
                Path = "/api/Search/NewsSearchAPI",
            };

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("autoCorrect=false");
            queryBuilder.Append("&pageNumber=1");
            queryBuilder.AppendFormat("&pageSize={0}", pageSize ?? 10);
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

                string jsonString = await results.Content.ReadAsStringAsync();
                var searchResults = JsonConvert.DeserializeObject<ContextualWebSearchResult>(jsonString);

                // Save the results to the DB
                using (SqlConnection con = sqlHelper.GetSqlConnection())
                {
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        const string insertSql = @"INSERT INTO NewsArticle (NewsArticleID, Url, Date, Title, RawText)
                                   VALUES (@NewsArticleID, @Url, @Date, @Title, @RawText)";

                        const string lookupSql = @"SELECT 1 FROM NewsArticle WHERE Url like @Url";

                        foreach (ContextualWebSearchResultItem item in searchResults.Value)
                        {
                            // Make sure the article doesn't already exist
                            if (!con.Query<int>(lookupSql, new { Url = item.Url }, transaction).Any())
                            {
                                con.Execute(

                                    insertSql,
                                    new
                                    {
                                        NewsArticleID = Guid.NewGuid(),
                                        Url = item.Url,
                                        Date = item.DatePublished,
                                        Title = item.Title,
                                        RawText = item.Body
                                    },
                                    transaction);
                            }
                        }

                        transaction.Commit();
                    }
                }

                return searchResults;
            }
        }
    }
}
