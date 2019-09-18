//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using MLServiceAPIML.Model.DataModels;


namespace MLServiceAPIML.ConsoleApp
{
    class Program
{
    //Machine Learning model to load and use for predictions
    private const string MODEL_FILEPATH = @"MLModel.zip";

    //Dataset to use for predictions 
    private const string DATA_FILEPATH = @"D:\Downloads\sentiment labelled sentences\sentiment labelled sentences\amazon_cells_labelledCopy.tsv";

    static void Main(string[] args)
    {
        MLContext mlContext = new MLContext();

        // Training code used by ML.NET CLI and AutoML to generate the model
        //ModelBuilder.CreateModel();

        ITransformer mlModel = mlContext.Model.Load(GetAbsolutePath(MODEL_FILEPATH), out DataViewSchema inputSchema);
        var predEngine = mlContext.Model.CreatePredictionEngine<NewsArticleSentimentAnalysisModelInput, NewsArticleSentimentAnalysisModelOutput>(mlModel);

        // Create sample data to do a single prediction with it 
        NewsArticleSentimentAnalysisModelInput sampleData = CreateSingleDataSample(mlContext, DATA_FILEPATH);
        //ModelInput sampleData = new ModelInput { SentimentText = "share reddit reddit small plane crashed new york long island saturday killing man woman board authorities said federal aviation authority said statement single-engine beechcraft plane crashed southold around m southold chief police martin flately said email two people aboard plane killed names immediately released flately said dog plane survived aircraft flying long island macarthur airport new bedford massachusetts experienced type mechanical failure crashed flately said advertisement advertise nzme jamesport fire chief mario carrera told newsday plane fully engulfed burning firefighters arrived kenneth cooper told newsday driving saw plane flying north low altitude said plane came sky sharp angle banked left said", Sentiment = false };

        // Try a single prediction
        NewsArticleSentimentAnalysisModelOutput predictionResult = predEngine.Predict(sampleData);

        Console.WriteLine($"Single Prediction --> Actual value: {sampleData.Sentiment} | Predicted value: {predictionResult.Prediction}");

        Console.WriteLine("=============== End of process, hit any key to finish ===============");
        Console.ReadKey();
    }

    // Method to load single row of data to try a single prediction
    // You can change this code and create your own sample data here (Hardcoded or from any source)
    private static NewsArticleSentimentAnalysisModelInput CreateSingleDataSample(MLContext mlContext, string dataFilePath)
    {
        // Read dataset to get a single row for trying a prediction          
        IDataView dataView = mlContext.Data.LoadFromTextFile<NewsArticleSentimentAnalysisModelInput>(
                                        path: dataFilePath,
                                        hasHeader: true,
                                        separatorChar: '\t',
                                        allowQuoting: true,
                                        allowSparse: false);

        // Here (ModelInput object) you could provide new test data, hardcoded or from the end-user application, instead of the row from the file.
        NewsArticleSentimentAnalysisModelInput sampleForPrediction = mlContext.Data.CreateEnumerable<NewsArticleSentimentAnalysisModelInput>(dataView, false)
                                                                    .First();
        return sampleForPrediction;
    }

    public static string GetAbsolutePath(string relativePath)
    {
        FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
        string assemblyFolderPath = _dataRoot.Directory.FullName;

        string fullPath = Path.Combine(assemblyFolderPath, relativePath);

        return fullPath;
    }
}
}
