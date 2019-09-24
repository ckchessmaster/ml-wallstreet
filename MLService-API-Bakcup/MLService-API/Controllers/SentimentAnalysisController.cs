using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using MLServiceAPIML.Model.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLServiceAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SentimentAnalysisController : Controller
    {
        private readonly PredictionEngine<NewsArticleSentimentAnalysisModelInput, NewsArticleSentimentAnalysisModelOutput> sentimentPredictionEngine;

        public SentimentAnalysisController(PredictionEngine<NewsArticleSentimentAnalysisModelInput, NewsArticleSentimentAnalysisModelOutput> sentimentPredictionEngine)
        {
            this.sentimentPredictionEngine = sentimentPredictionEngine;
        }

        [Route("predict")]
        public IActionResult GetPrediction([FromQuery(Name = "inputText")]string inputText)
        {
            var result = sentimentPredictionEngine.Predict(new NewsArticleSentimentAnalysisModelInput { SentimentText = inputText });

            return new JsonResult(new { Result = result.Prediction });
        }
    }
}
