using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MLWallstreetUI.Data.Models
{
    public class SentimentAnalysisModel
    {
        [FileExtensions(Extensions = "csv")]
        public IFormFile TrainingData { get; set; }
    }
}
