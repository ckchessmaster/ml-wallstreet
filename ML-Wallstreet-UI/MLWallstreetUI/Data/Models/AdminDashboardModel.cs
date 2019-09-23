using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLWallstreetUI.Data.Models
{
    public class AdminDashboardModel
    {
        public DataRetrievalModel DataRetrievalModelBinding { get; set; }

        public SentimentAnalysisModel SentimentAnalysisModelBinding { get; set; }
    }
}
