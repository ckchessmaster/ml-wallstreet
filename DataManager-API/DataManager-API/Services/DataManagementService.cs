using Dapper;
using DataManagerAPI.Enums;
using DataManagerAPI.Models.ServiceModels;
using DataManagerAPI.Models.ServiceModels.ContextualWebSearch;
using DataManagerAPI.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataManagerAPI.Services
{
    public class DataManagementService
    {
        private readonly HttpClient client;

        private readonly SqlHelper sqlHelper;

        private readonly IConfiguration config;

        public DataManagementService(
            HttpClient client,
            SqlHelper sqlHelper,
            IConfiguration config)
        {
            this.client = client;
            this.sqlHelper = sqlHelper;
            this.config = config;
        }

        public async Task<List<ContextualWebSearchResult>> LoadNewData(DateTime startDate, DateTime endDate, string searchQuery, int? pageSize)
        {
            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "contextualwebsearch-websearch-v1.p.rapidapi.com",
                Path = "/api/Search/NewsSearchAPI",
            };

            List<ContextualWebSearchResult> finalResults = new List<ContextualWebSearchResult>();

            DateTime intialStartDate = startDate;
            DateTime initialEndDate = endDate;

            for (double i = 0; i <= (initialEndDate - intialStartDate).TotalDays; i++)
            {
                startDate = intialStartDate.AddDays(i);
                endDate = intialStartDate.AddDays(i + 1);

                var queryBuilder = new StringBuilder();
                queryBuilder.Append("autoCorrect=false");
                queryBuilder.Append("&pageNumber=1");
                queryBuilder.AppendFormat("&pageSize={0}", pageSize ?? 10);
                queryBuilder.AppendFormat("&q={0}", string.IsNullOrEmpty(searchQuery) ? "news" : searchQuery);
                queryBuilder.Append("&safeSearch=false");
                queryBuilder.AppendFormat("&fromPublishedDate={0}", startDate.ToShortDateString());
                queryBuilder.AppendFormat("&toPublishedDate={0}", endDate.ToShortDateString());

                uriBuilder.Query = queryBuilder.ToString();

                using (var results = await client.GetAsync(uriBuilder.Uri))
                {
                    results.EnsureSuccessStatusCode();

                    string jsonString = await results.Content.ReadAsStringAsync();
                    var searchResults = JsonConvert.DeserializeObject<ContextualWebSearchResult>(jsonString);

                    finalResults.Append(searchResults);

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
                }
            }

            return finalResults;
        }

        public async Task CleanData()
        {
            using (SqlConnection con = sqlHelper.GetSqlConnection())
            {
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }

                // Load all data that has not previously been cleaned
                string sql = "SELECT NewsArticleID, RawText FROM NewsArticle WHERE CleanText IS NULL";

                var dirtyData = await con.QueryAsync<NewsArticle>(sql);

                // Clean the text
                foreach (NewsArticle article in dirtyData)
                {
                    article.CleanText = Clean(article.RawText);
                }

                // Update the database
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    sql = "UPDATE NewsArticle SET CleanText = @CleanText WHERE NewsArticleID = @NewsArticleID";
                    foreach (NewsArticle article in dirtyData)
                    {
                        await con.ExecuteAsync(sql, new { CleanText = article.CleanText, NewsArticleID = article.NewsArticleID }, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        private string Clean(string text)
        {
            string final = text;

            // Remove all non alpha characters
            var rgx = new Regex("(\r\n|\n|\r|\\n)");
            final = rgx.Replace(text, " ");

            rgx = new Regex("[^a-zA-Z -]");
            final = rgx.Replace(final, " ");

            rgx = new Regex("\\s+");
            final = rgx.Replace(final, " ");

            // Make everything lowercase
            final = final.ToLower();

            // Remove stopwords
            var words = final.Split(" ").ToList();

            var stopwords = new List<string>();
            using (var file = new StreamReader(config.GetValue<string>("StopWordsLocation")))
            {
                string line;
                while((line = file.ReadLine()) != null) 
                {
                    stopwords.Add(line);
                }
            }

            words = words.Where(word => !stopwords.Contains(word)).ToList();

            // Combine everything back together
            final = String.Join(" ", words);

            return final;
        }
    }
}
