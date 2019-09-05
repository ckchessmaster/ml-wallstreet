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
        private readonly DataLoaderService dataLoaderService;

        public NewsController(DataLoaderService dataLoaderService)
        {
            this.dataLoaderService = dataLoaderService;
        }

        [HttpPost]
        [Route("LoadNewData")]
        public async Task<IActionResult> LoadNewData([FromBody]LoadNewDataRequestModel requestModel)
        {
            var results = await dataLoaderService.LoadNewData(
                requestModel.StartDate, 
                requestModel.EndDate, 
                requestModel.SearchQuery, 
                requestModel.PageSize);

            return new JsonResult(new { Results = results });
        }
    }
}
