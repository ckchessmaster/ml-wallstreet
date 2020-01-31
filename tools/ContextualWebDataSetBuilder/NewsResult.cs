using System;
using System.Collections.Generic;
using System.Text;

namespace ContextualWebDataSetBuilder
{
    public class NewsResult
    {

        public string DidUMean { get; set; }

        public int TotalCount { get; set; }

        public List<string> RelatedSearch { get; set; }

        public List<NewsItem> Value { get; set; }
    }
}
