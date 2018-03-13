using System.Collections.Generic;
using SlidableLive.Models.Live;

namespace SlidableLive.Models.Shows
{
    public class ShowListViewModel
    {
        public string Tag { get; set; }
        public IList<Show> Shows { get; set; }
    }
}