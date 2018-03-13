using System.Threading;
using System.Threading.Tasks;
using SlidableLive.Models.Live;
using SlidableLive.Models.Present;

namespace SlidableLive.Clients
{
    public interface IShowsClient
    {
        Task<bool> ShowSlide(string presenter, string slug, int number, CancellationToken ct = default);
        Task<Show> Get(string presenter, string slug, CancellationToken ct = default);
        Task<Slide> GetSlide(string presenter, string slug, int number, CancellationToken ct = default);
        Task<Show> Start(StartShow startShow, CancellationToken ct = default);
        Task<Show> GetLatest(string presenter, CancellationToken ct = default);
        Task<Slide> GetLatestSlide(string presenter, string slug, CancellationToken ct = default);
    }
}