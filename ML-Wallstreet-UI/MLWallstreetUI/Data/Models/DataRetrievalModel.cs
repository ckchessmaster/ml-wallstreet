using MLWallstreetUI.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLWallstreetUI.Data.Models
{
    public class DataRetrievalModel
    {
        public DataRetrievalSearchApiType ApiType { get; set; }

        public string SearchQuery { get; set; }

        public int PageSize { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool MyProperty { get; set; }
    }
}
