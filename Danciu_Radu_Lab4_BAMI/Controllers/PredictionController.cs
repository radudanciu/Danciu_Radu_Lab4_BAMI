using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using static Danciu_Radu_Lab4_BAMI.PricePredictionModel;

namespace Danciu_Radu_Lab4_BAMI.Controllers
{
    public class PredictionController : Controller
    {
        public IActionResult Price(ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();
            // Create predection engine related to the loaded train model
            var modelPath = @"C:\Users\radud\source\repos\Danciu_Radu_Lab4_BAMI\Danciu_Radu_Lab4_BAMI\PricePredictionModel.mlnet";
            ITransformer mlModel = mlContext.Model.Load(modelPath, out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput,
           ModelOutput>(mlModel);
            // Try model on sample data to predict fair price
            ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;
            return View(input);

        }
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

    }
}
