using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SlidableLive.Models.Shows;
using SlidableLive.Shows;

namespace SlidableLive.Controllers
{
    [Route("shows")]
    public class ShowsController : Controller
    {
        private readonly IShowService _service;

        public ShowsController(IShowService service)
        {
            _service = service;
        }

        [HttpGet("tag")]
        public async Task<IActionResult> FindByTag([FromQuery]string tag)
        {
            var shows = await _service.FindByTag(tag);
            return View(new ShowListViewModel
            {
                Shows = shows,
                Tag = tag
            });
        }
    }
}