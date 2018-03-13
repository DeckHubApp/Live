using System.Collections.Generic;
using System.Threading.Tasks;
using SlidableLive.Models.Live;

namespace SlidableLive.Shows
{
    public interface IShowService
    {
        Task<Show> GetLatest(string presenter);
        Task<IList<Show>> FindByTag(string tag);
    }
}