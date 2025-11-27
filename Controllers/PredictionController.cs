using Hulujan_Iulia_Petruta_lab4M;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using  PriceModel=Hulujan_Iulia_Petruta_lab4M.PricePredictionModel; 
using DurationModel= Hulujan_Iulia_Petruta_lab4M.DurationPredictionModel;

namespace Hulujan_Iulia_Petruta_lab4M.Controllers
{
    public class PredictionController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Price(PriceModel.ModelInput input)
        {
            //Load the model
            MLContext mlContext = new MLContext();
            // Create prediction engine related to the loaded train model
            ITransformer mlModel = mlContext.Model.Load(@"..\Hulujan_Iulia_Petruta_lab4M\PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<PriceModel.ModelInput, PriceModel.ModelOutput>(mlModel);

            //Try model on sample data to predict fair price
            PriceModel.ModelOutput result = predEngine.Predict(input);

            ViewBag.Price = result.Score;
            return View(input);
        }
        
        public IActionResult Duration(DurationModel.ModelInput input)
        {
            MLContext mlContext = new MLContext();

            ITransformer mlModel = mlContext.Model.Load(@"..\Hulujan_Iulia_Petruta_lab4M\DurationPredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<DurationModel.ModelInput, DurationModel.ModelOutput>(mlModel);

            DurationModel.ModelOutput result = predEngine.Predict(input);

            ViewBag.Duration = result.Score;
            return View(input);

        }

    }
}
