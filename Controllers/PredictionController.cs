using Hulujan_Iulia_Petruta_lab4M;
using Hulujan_Iulia_Petruta_lab4M.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using static Plotly.NET.StyleParam;
using DurationModel= Hulujan_Iulia_Petruta_lab4M.DurationPredictionModel;
using  PriceModel=Hulujan_Iulia_Petruta_lab4M.PricePredictionModel; 

namespace Hulujan_Iulia_Petruta_lab4M.Controllers
{
    public class PredictionController : Controller
    {
        private readonly Hulujan_Iulia_Petruta_lab4M.Data.AppDbContext _context;

        public PredictionController(Hulujan_Iulia_Petruta_lab4M.Data.AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Price()
        {
            return View(new PricePredictionModel.ModelInput());
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> Price(PriceModel.ModelInput input)
        {

            //Load the model
            MLContext mlContext = new MLContext();
            // Create prediction engine related to the loaded train model
            ITransformer mlModel = mlContext.Model.Load(@"..\Hulujan_Iulia_Petruta_lab4M\PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<PriceModel.ModelInput, PriceModel.ModelOutput>(mlModel);

            //Try model on sample data to predict fair price
            PriceModel.ModelOutput result = predEngine.Predict(input);

            ViewBag.Price = result.Score;


            var history = new PredictionHistory
            {
                PassengerCount = input.Passenger_count,
                TripTimeInSecs = input.Trip_time_in_secs,
                TripDistance = input.Trip_distance,
                PaymentType = input.Payment_type,
                PredictedPrice = result.Score,
                CreatedAt = DateTime.Now
            };
            _context.PredictionHistories.Add(history);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Prediction saved successfully!";

            return View(input);
        }

        
        [HttpGet]
        public async Task<IActionResult> History(string? paymentType,
float? minPrice,
float? maxPrice,
DateTime? startDate,
DateTime? endDate,
string? sortOrder)
        {
            var query = _context.PredictionHistories.AsQueryable();
            if (!string.IsNullOrEmpty(paymentType))
            {
                query = query.Where(p => p.PaymentType == paymentType);
            }
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice <= maxPrice.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= endDate.Value);
            }


            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.PredictedPrice),
                "price_desc" => query.OrderByDescending(p => p.PredictedPrice),
                "date_asc" => query.OrderBy(p => p.CreatedAt),
                "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.PredictedPrice) //sortare default
            };

            ViewBag.CurrentPaymentType = paymentType;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;


            var result = await query.ToListAsync();
            return View(result);

            //var history = await _context.PredictionHistories
            //.OrderByDescending(p => p.CreatedAt)
            //.ToListAsync();
            //return View(history);
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
