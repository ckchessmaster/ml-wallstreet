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
using MLWallstreetUI.Services;
using Newtonsoft.Json;

namespace MLWallstreetUI.Pages
{
    [Authorize(Policy = "UsersMustBeActive")]
    public class IndexModel : PageModel
    {
        public class DataRetrievalModel
        {
            public DataRetrievalSearchApiType ApiType { get; set; }

            public string SearchQuery { get; set; }

            public int PageSize { get; set; }

            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }
        }

        private readonly ApiService apiService;

        public IndexModel(ApiService apiService)
        {
            this.apiService = apiService;
        }

        [BindProperty]
        public DataRetrievalModel DataRetrievalModelBinding { get; set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var dataManager = await apiService.GetApi(ServiceApiType.DataManagerApi);

            string jsonString = JsonConvert.SerializeObject(DataRetrievalModelBinding);

            var results = await dataManager.Client.PostAsync(dataManager.BaseUrl + "news/loadNewData", new StringContent(jsonString, Encoding.UTF8, "application/json"));

            results.EnsureSuccessStatusCode();

            return RedirectToPage("/Index");
        }
    }
}
