using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MLWallstreetUI.Data.Enums;
using MLWallstreetUI.Data.Models;
using MLWallstreetUI.Services;
using Newtonsoft.Json;
using static MLWallstreetUI.Data.Models.AdminDashboardModel;

namespace MLWallstreetUI.Pages
{
    [Authorize(Policy = "UsersMustBeActive")]
    public class IndexModel : PageModel
    {
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
            CleanData();

            return RedirectToPage("/Index");
        }

        public IActionResult OnPostSentimentTrainer()
        {
            var trainingData = AdminDashboardModel.SentimentAnalysisModelBinding.TrainingData;

            // Make sure this is valid data
            if (trainingData == null || !allowedFileTypes.Contains(trainingData.ContentType))
            {
                throw new Exception("Invalid Training Data!");
            }

            return RedirectToPage("/Index");
        }

        private async Task CleanData()
        {
            var dataManager = await apiService.GetApi(ServiceApiType.DataManagerApi);

            await dataManager.Client.PostAsync(dataManager.BaseUrl + "news/clean", new StringContent("{}", Encoding.UTF8, "application/json"));
        }
    }
}
