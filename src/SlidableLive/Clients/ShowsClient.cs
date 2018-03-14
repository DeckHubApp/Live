using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SlidableLive.Models.Present;
using Show = SlidableLive.Models.Live.Show;
using Slide = SlidableLive.Models.Live.Slide;

namespace SlidableLive.Clients
{
    public class ShowsClient : IShowsClient
    {
        private readonly HttpClient _http;

        public ShowsClient(IOptions<Options.ServiceOptions> options)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(options.Value.Shows.BaseUrl)
            };
        }

        public async Task<Show> Start(StartShow startShow, CancellationToken ct = default)
        {
            var show = new Show
            {
                Place = startShow.Place,
                Presenter = startShow.Presenter,
                Slug = startShow.Slug,
                Time = startShow.Time,
                HighestSlideShown = 0
            };
            var response = await _http.PostJsonAsync("/shows/start", show, ct: ct).ConfigureAwait(false);
            return await response.Deserialize<Show>();
        }

        public async Task<Show> GetLatest(string presenter, CancellationToken ct = default)
        {
            var response = await _http.GetAsync($"/shows/find/by/{presenter}/latest", ct).ConfigureAwait(false);
            return await response.Deserialize<Show>();
        }

        public async Task<Show> Get(string presenter, string slug, CancellationToken ct = default)
        {
            var response = await _http.GetAsync($"/shows/{presenter}/{slug}", ct).ConfigureAwait(false);
            return await response.Deserialize<Show>();
        }

        public async Task<IList<Show>> FindByTag(string tag, CancellationToken ct = default)
        {
            var response = await _http.GetAsync($"/shows/find-by-tag/{tag}", ct).ConfigureAwait(false);
            return await response.Deserialize<Show[]>();
        }
    }
}