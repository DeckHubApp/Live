﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SlidableLive.Models.Notes;

namespace SlidableLive.Clients
{
    public class NotesClient : INotesClient
    {
        private readonly HttpClient _http;

        public NotesClient(IOptions<Options.ServiceOptions> options)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(options.Value.Notes.BaseUrl)
            };
        }

        public async Task<Note> GetNotes(string user, string presenter, string slug, int number, CancellationToken ct = default(CancellationToken))
        {
            var response = await _http.GetAsync($"{user}/{presenter}/{slug}/{number}", ct)
                .ConfigureAwait(false);
            return await response.Deserialize<Note>();
        }

        public async Task<bool> SaveNotes(string user, string presenter, string slug, int number, Note note, CancellationToken ct = default(CancellationToken))
        {
            var response = await _http.PutJsonAsync($"{user}/{presenter}/{slug}/{number}", note, ct)
                .ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return true;
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
            throw new UpstreamServiceException(response.StatusCode, response.ReasonPhrase);
        }
    }
}