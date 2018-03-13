﻿namespace SlidableLive.Models.Live
{
    public class Slide
    {
        public string Presenter { get; set; }
        public string Slug { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public string Layout { get; set; }
        public string Html { get; set; }
        public bool HasBeenShown { get; set; }
    }
}