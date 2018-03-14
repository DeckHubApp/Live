using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SlidableLive.Clients;
using SlidableLive.Models.Shows;

namespace SlidableLive.Controllers
{
    [Route("shows")]
    public class ShowsController : Controller
    {
        private readonly IShowsClient _client;

        public ShowsController(IShowsClient client)
        {
            _client = client;
        }

        [HttpGet("tag")]
        public async Task<IActionResult> FindByTag([FromQuery]string tag)
        {
            var shows = await _client.FindByTag(tag);
            return View(new ShowListViewModel
            {
                Shows = shows,
                Tag = tag
            });
        }
    }
}