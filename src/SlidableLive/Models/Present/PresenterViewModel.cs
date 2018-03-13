using System.Collections.Generic;
using SlidableLive.Models.Live;
using SlidableLive.Models.Questions;

namespace SlidableLive.Models.Present
{
    public class PresenterViewModel
    {
        public Show Show { get; set; }
        public List<Question> Questions { get; set; }
    }
}