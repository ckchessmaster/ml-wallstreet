using System;
using System.Collections.Generic;
using System.Text;

namespace ContextualWebDataSetBuilder
{
    public class NewsItem
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
