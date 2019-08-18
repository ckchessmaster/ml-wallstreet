using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManagerAPI.Models.ServiceModels.ContextualWebSearch
{
    public class ContextualWebSearchResultItem
    {
        public string Title { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public string Keywords { get; set; }

        public string Language { get; set; }

        public bool IsSafe { get; set; }

        public DateTime DatePublished { get; set; }
    }
}
