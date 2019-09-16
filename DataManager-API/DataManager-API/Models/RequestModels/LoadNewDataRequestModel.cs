using System;

namespace DataManagerAPI.Models.RequestModels
{
    public class LoadNewDataRequestModel
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string SearchQuery { get; set; }

        public int? PageSize { get; set; }
    }
}
