using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text.Json;

public class FootballService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private int _remainingRequests = 10;
    private DateTime _resetTime = DateTime.UtcNow;

    public FootballService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    private async Task<(JsonDocument? Data, string? ErrorMessage)> FetchDataAsync(string endpoint, int cacheDuration = 60)
    {
        if (_cache.TryGetValue(endpoint, out JsonDocument cachedData))
        {
            return (cachedData, null); // ✅ Serve cached data
        }

        // ⏳ Check rate limits
        if (DateTime.UtcNow < _resetTime && _remainingRequests <= 2)
        {
            int waitTime = (int)(_resetTime - DateTime.UtcNow).TotalSeconds;
            return (null, $"Rate limit reached. Try again in {waitTime} seconds.");
        }

        try
        {
            var response = await _httpClient.GetAsync(endpoint);

            // 🛑 Read API rate limit headers
            if (response.Headers.TryGetValues("X-Requests-Available-Minute", out var remainingReqValues))
            {
                _remainingRequests = int.Parse(remainingReqValues.First());
            }
            if (response.Headers.TryGetValues("X-RequestCounter-Reset", out var resetValues))
            {
                _resetTime = DateTime.UtcNow.AddSeconds(int.Parse(resetValues.First()));
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"⚠️ API returned 400: {errorMessage}");

                return (null, $"API Error: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);

            // ✅ Cache response
            _cache.Set(endpoint, jsonDocument, TimeSpan.FromSeconds(cacheDuration));

            return (jsonDocument, null);
        }
        catch (Exception ex)
        {
            return (null, $"Exception: {ex.Message}");
        }
    }

    public async Task<(JsonDocument? Data, string? ErrorMessage)> GetCompetitionsAsync()
    {
        return await FetchDataAsync("competitions");
    }

    public async Task<(JsonDocument? Data, string? ErrorMessage)> GetCompetitionDetailsAsync(string leagueCode)
    {
        return await FetchDataAsync($"competitions/{leagueCode}");
    }

    public async Task<(JsonDocument? Data, string? ErrorMessage)> GetStandingsAsync(string leagueCode)
    {
        return await FetchDataAsync($"competitions/{leagueCode}/standings", 120);
    }

    public async Task<(JsonDocument? Data, string? ErrorMessage)> GetTopScorersAsync(string leagueCode)
    {
        return await FetchDataAsync($"competitions/{leagueCode}/scorers", 120);
    }
}
