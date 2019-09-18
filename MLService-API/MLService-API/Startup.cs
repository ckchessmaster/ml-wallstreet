using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ML;
using MLServiceAPIML.Model.DataModels;

namespace MLServiceAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureAuthentication(services);

            services.AddMvc();

            ConfigureML(services);
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            // Configure jwt authentication
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetValue<string>("Security:SigningKey"))),
                    ValidateIssuer = true,
                    ValidIssuer = "MLServiceAPI",
                    ValidateAudience = true,
                    ValidAudience = "MLServiceAPI"
                };
            });
        }

        private void ConfigureML(IServiceCollection services)
        {
            // Configure the news article sentiment analysis engine
            MLContext mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load("MLModel.zip", out var modelInputSchema);
            services.AddSingleton(mlContext.Model.CreatePredictionEngine<NewsArticleSentimentAnalysisModelInput, NewsArticleSentimentAnalysisModelOutput>(mlModel));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}


//public static void ConsumeModel()
//{
//    // Load the model
//    MLContext mlContext = new MLContext();

//    ITransformer mlModel = mlContext.Model.Load("MLModel.zip", out var modelInputSchema);

//    var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

//    // Use the code below to add input data
//    var input = new ModelInput();
//    input.SentimentText = "Type your sentiment";

//    // Try model on sample data
//    // True is toxic, false is non-toxic
//    ModelOutput result = predEngine.Predict(input);

//    Console.WriteLine($"Text: {input.SentimentText} | Prediction: {(Convert.ToBoolean(result.Prediction) ? "Toxic" : "Non Toxic")} sentiment");
//}