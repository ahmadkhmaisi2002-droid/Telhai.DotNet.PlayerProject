using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Telhai.DotNet.PlayerProject.Services
{
    public class ItunesService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<ItunesResponse?> SearchSongAsync(string query, CancellationToken token)
        {
            string url = $"https://itunes.apple.com/search?term={query}&limit=1";

            var response = await _httpClient.GetAsync(url, token);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(token);

            return JsonSerializer.Deserialize<ItunesResponse>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        }
    }

    public class ItunesResponse
    {
        public int ResultCount { get; set; }
        public ItunesSong[] Results { get; set; } = Array.Empty<ItunesSong>();
    }

    public class ItunesSong
    {
        public string? TrackName { get; set; }
        public string? ArtistName { get; set; }
        public string? CollectionName { get; set; }
        public string? ArtworkUrl100 { get; set; }
    }
}
