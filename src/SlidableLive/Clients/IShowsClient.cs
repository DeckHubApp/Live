using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SlidableLive.Models.Live;

namespace SlidableLive.Clients
{
    public interface IShowsClient
    {
        Task<Show> Get(string presenter, string slug, CancellationToken ct = default);
        Task<Show> GetLatest(string presenter, CancellationToken ct = default);
        Task<IList<Show>> FindByTag(string tag, CancellationToken ct = default);
    }
}