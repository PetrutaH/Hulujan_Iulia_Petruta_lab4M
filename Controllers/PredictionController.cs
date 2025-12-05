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

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // 1. Numărul total de predicții
            var totalPredictions = await _context.PredictionHistories.CountAsync();
            // 2. Preț mediu per tip de plată + număr de predicții per tip
            var paymentTypeStats = await _context.PredictionHistories
            .GroupBy(p => p.PaymentType)
            .Select(g => new PaymentTypeStat
            {
                PaymentType = g.Key,
                AveragePrice = g.Average(x => x.PredictedPrice),
                Count = g.Count()
            })
            .ToListAsync();
            // 3. Distribuția prețurilor pe intervale (buckets)
            // Definim intervalele: 0-10, 10-20, 20-30, 30-50, >50 (exemplu)
            var allPredictions = await _context.PredictionHistories
            .Select(p => p.PredictedPrice)
            .ToListAsync();
            var buckets = new List<PriceBucketStat>
 {
 new PriceBucketStat { Label = "0 - 10" },
 new PriceBucketStat { Label = "10 - 20" },
 new PriceBucketStat { Label = "20 - 30" },
 new PriceBucketStat { Label = "30 - 50" },
 new PriceBucketStat { Label = "> 50" }
 };
            foreach (var price in allPredictions)
            {
                if (price < 10)
                    buckets[0].Count++;
                else if (price < 20)
                    buckets[1].Count++;
                else if (price < 30)
                    buckets[2].Count++;
                else if (price < 50)
                    buckets[3].Count++;
                else
                    buckets[4].Count++;
            }
            // 4. Construim ViewModel-ul
            var vm = new DashboardViewModel
            {
                TotalPredictions = totalPredictions,
                PaymentTypeStats = paymentTypeStats,
                PriceBuckets = buckets


            };
            return View(vm);
        }
    }
}
