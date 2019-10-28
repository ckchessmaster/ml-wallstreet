using System.Collections.Generic;

namespace DataManagerAPI.Models.ServiceModels.ContextualWebSearch
{
    public class ContextualWebSearchResult
    {
        public string Type { get; set; }

        public string DidYouMean { get; set; }

        public int TotalCount { get; set; }

        public IEnumerable<string> RelatedSearch { get; set; }

        public IEnumerable<ContextualWebSearchResultItem> Value { get; set; }
    }
}
