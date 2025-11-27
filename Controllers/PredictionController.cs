using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using static Hulujan_Iulia_Petruta_lab4M.PricePredictionModel; 

namespace Hulujan_Iulia_Petruta_lab4M.Controllers
{
    public class PredictionController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Price(ModelInput input)
        {
            //Load the model
            MLContext mlContext = new MLContext();
            // Create prediction engine related to the loaded train model
            ITransformer mlModel = mlContext.Model.Load(@"..\Hulujan_Iulia_Petruta_lab4M\PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            //Try model on sample data to predict fair price
            ModelOutput result = predEngine.Predict(input);

            ViewBag.Price = result.Score;
            return View(input);
        }

    }
}
