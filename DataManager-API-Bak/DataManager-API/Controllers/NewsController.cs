using DataManagerAPI.Enums;
using DataManagerAPI.Models.RequestModels;
using DataManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManagerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class NewsController : Controller
    {
        private readonly DataManagementService dataManagementService;

        public NewsController(DataManagementService dataManagementService)
        {
            this.dataManagementService = dataManagementService;
        }

        [HttpPost]
        [Route("LoadNewData")]
        public async Task<IActionResult> LoadNewData([FromBody]LoadNewDataRequestModel requestModel)
        {
            var results = await dataManagementService.LoadNewData(
                requestModel.StartDate, 
                requestModel.EndDate, 
                requestModel.SearchQuery, 
                requestModel.PageSize);

            return new JsonResult(new { Results = results });
        }

        [HttpPost]
        [Route("Clean")]
        public async Task<IActionResult> CleanData()
        {
            await dataManagementService.CleanData();

            return new JsonResult(new { Results = "Success" });
        }
    }
}
