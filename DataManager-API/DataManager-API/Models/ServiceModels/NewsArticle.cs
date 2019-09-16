using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataManagerAPI.Models.ServiceModels
{
    public class NewsArticle
    {
        public Guid NewsArticleID { get; set; }

        public string Url { get; set; }

        public DateTime Date { get; set; }

        public string Title { get; set; }

        public string RawText { get; set; }

        public string CleanText { get; set; }
    }
}
