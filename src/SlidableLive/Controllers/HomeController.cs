using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SlidableLive.Models;
using SlidableLive.Services;

namespace SlidableLive.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserInfo _userInfo;
        public HomeController(IUserInfo userInfo)
        {
            _userInfo = userInfo;
        }

        public IActionResult Index()
        {
            var claims = User.Claims.ToList();
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
