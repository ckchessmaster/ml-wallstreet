using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MLWallstreetUI.Data.Enums;
using MLWallstreetUI.Data.Models;
using MLWallstreetUI.Services;
using Newtonsoft.Json;

namespace MLWallstreetUI.Pages
{
    [DisableRequestSizeLimit]
    [Authorize(Policy = "UsersMustBeActive")]
    public class IndexModel : PageModel
    {
        private class SentitmentTrainingDataModel
        {
            public int Result { get; set; }

            public string Text { get; set; }
        }

        private class Accuracy
        {
            public decimal Average { get; set; }

            public decimal StandardDeviation { get; set; }
        }

        private readonly ApiService apiService; 

        public IndexModel(ApiService apiService)
        {
            this.apiService = apiService;
        }

        [BindProperty]
        public AdminDashboardModel AdminDashboardModel { get; set; }

        private readonly string[] allowedFileTypes = { "text/csv", "application/vnd.ms-excel" };

        public IActionResult OnPostDataRetriever()
        {
            RetrieveData();

            return RedirectToPage("/Index");
        }

        private async Task RetrieveData()
        {
            var dataManager = await apiService.GetApi(ServiceApiType.DataManagerApi);
            string jsonString = JsonConvert.SerializeObject(AdminDashboardModel.DataRetrievalModelBinding);

            await dataManager.Client.PostAsync(dataManager.BaseUrl + "news/loadNewData", new StringContent(jsonString, Encoding.UTF8, "application/json"));
        }

        public IActionResult OnPostDataCleaner()
        {
            _ = CleanData();

            return RedirectToPage("/Index");
        }

        private async Task CleanData()
        {
            var dataManager = await apiService.GetApi(ServiceApiType.DataManagerApi);

            await dataManager.Client.PostAsync(dataManager.BaseUrl + "news/clean", new StringContent("{}", Encoding.UTF8, "application/json"));
        }

        public IActionResult OnPostSentimentTrainer()
        {
            var trainingData = AdminDashboardModel.SentimentAnalysisModelBinding.TrainingData;

            // Make sure this is valid data
            if (trainingData == null || !allowedFileTypes.Contains(trainingData.ContentType))
            {
                throw new Exception("Invalid Training Data!");
            }

            var result = new StringBuilder();
            using (var reader = new StreamReader(trainingData.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    result.AppendLine(reader.ReadLine());
                }
            }

            List<SentitmentTrainingDataModel> data;
            using (var textReader = new StringReader(result.ToString()))
            {
                using (var csvr = new CsvReader(textReader))
                {
                    data = csvr.GetRecords<SentitmentTrainingDataModel>().ToList();
                }
            }

            _ = TrainSentimentModel(data);

            return RedirectToPage("/Index");
        }

        private async Task TrainSentimentModel(List<SentitmentTrainingDataModel> data)
        {
            var mlService = await apiService.GetApi(ServiceApiType.MLServiceApi);
           
            string jsonString = JsonConvert.SerializeObject(new
            {
                TestSetSize = 0.25m,
                TrainingData = data
            });

            // Check if training is already in progress
            var result = await mlService.Client.GetAsync(mlService.BaseUrl + "sentiment/isTraining");

            result.EnsureSuccessStatusCode();

            string resultString = await result.Content.ReadAsStringAsync();
            bool isTrainingProgress = JsonConvert.DeserializeObject<bool>(resultString);

            if (!isTrainingProgress)
            {
                result = await mlService.Client.PostAsync(mlService.BaseUrl + "sentiment/train", new StringContent(jsonString, Encoding.UTF8, "application/json"));
                result.EnsureSuccessStatusCode();
            }
        }
    }
}
