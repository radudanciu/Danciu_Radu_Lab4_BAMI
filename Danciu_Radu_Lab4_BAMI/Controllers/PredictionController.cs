using Danciu_Radu_Lab4_BAMI.Models;
using Danciu_Radu_Lab4_BAMI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System;
using System.Threading.Tasks;
using static Danciu_Radu_Lab4_BAMI.PricePredictionModel;

namespace Danciu_Radu_Lab4_BAMI.Controllers
{
    public class PredictionController : Controller
    {
        private readonly AppDbContext _context;

        public PredictionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Price()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Price(ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();

            // Recomandat: pune modelul în proiect și folosește AppContext.BaseDirectory,
            // dar păstrez path-ul tău absolut ca să îți meargă imediat.
            var modelPath = @"C:\Users\radud\source\repos\Danciu_Radu_Lab4_BAMI\Danciu_Radu_Lab4_BAMI\PricePredictionModel.mlnet";

            ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            // Predict
            ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;

            // Save history
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

            return View(input);
        }

        [HttpGet]
        public IActionResult Time()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Time(Danciu_Radu_Lab4_BAMI.DurationPredictionModel.ModelInput input)
        {
            MLContext mlContext = new MLContext();

            var modelPath = @"C:\Users\radud\source\repos\Danciu_Radu_Lab4_BAMI\Danciu_Radu_Lab4_BAMI\DurationPredictionModel.mlnet";
            ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);

            var predEngine = mlContext.Model.CreatePredictionEngine<
                Danciu_Radu_Lab4_BAMI.DurationPredictionModel.ModelInput,
                Danciu_Radu_Lab4_BAMI.DurationPredictionModel.ModelOutput>(mlModel);

            var result = predEngine.Predict(input);

            ViewBag.Time = result.Score;
            ViewBag.TimeMinutes = result.Score / 60.0f;

            return View(input);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var history = await _context.PredictionHistories
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(history);
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            
            var query = _context.PredictionHistories.AsQueryable();

        
            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedAt.Date >= fromDate.Value.Date);

           
            if (toDate.HasValue)
                query = query.Where(p => p.CreatedAt.Date <= toDate.Value.Date);

            
            var totalPredictions = await query.CountAsync();

          
            var paymentTypeStats = await query
                .GroupBy(p => p.PaymentType)
                .Select(g => new PaymentTypeStat
                {
                    PaymentType = g.Key,
                    AveragePrice = g.Average(x => x.PredictedPrice),
                    Count = g.Count()
                })
                .ToListAsync();

      
            var allPredictions = await query
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
                if (price < 10) buckets[0].Count++;
                else if (price < 20) buckets[1].Count++;
                else if (price < 30) buckets[2].Count++;
                else if (price < 50) buckets[3].Count++;
                else buckets[4].Count++;
            }

            
            var vm = new DashboardViewModel
            {
                TotalPredictions = totalPredictions,
                PaymentTypeStats = paymentTypeStats,
                PriceBuckets = buckets,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(vm);
        }

    }
}
