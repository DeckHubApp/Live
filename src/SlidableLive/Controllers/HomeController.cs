using System.Diagnostics;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Timer;
using Microsoft.AspNetCore.Mvc;
using SlidableLive.Models;

namespace SlidableLive.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private static readonly CounterOptions Counter = new CounterOptions
        {
            Name = "Index",
            MeasurementUnit = Unit.Calls
        };

        private static readonly TimerOptions Timer = new TimerOptions
        {
            Name = "IndexTimer",
            DurationUnit = TimeUnit.Milliseconds
        };

        private readonly IMetrics _metrics;

        public HomeController(IMetrics metrics)
        {
            _metrics = metrics;
        }

        [HttpGet]
        public IActionResult Index()
        {
            _metrics.Measure.Counter.Increment(Counter);
            using (_metrics.Measure.Timer.Time(Timer))
            {
                return View();
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
