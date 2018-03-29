using System.Diagnostics;
using System.Linq;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Timer;
using Microsoft.AspNetCore.Mvc;
using SlidableLive.Models;
using SlidableLive.Services;

namespace SlidableLive.Controllers
{
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

        private readonly IUserInfo _userInfo;
        private readonly IMetrics _metrics;

        public HomeController(IUserInfo userInfo, IMetrics metrics)
        {
            _userInfo = userInfo;
            _metrics = metrics;
        }

        public IActionResult Index()
        {
            _metrics.Measure.Counter.Increment(Counter);
            using (_metrics.Measure.Timer.Time(Timer))
            {
                var claims = User.Claims.ToList();
                return View();
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
