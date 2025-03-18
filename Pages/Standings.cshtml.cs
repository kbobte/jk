using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Threading.Tasks;

public class StandingsModel : PageModel
{
    private readonly FootballService _footballService;
    
    public JsonDocument? StandingsData { get; private set; }
    public string? ErrorMessage { get; private set; }

    public StandingsModel(FootballService footballService)
    {
        _footballService = footballService;
    }

    public async Task OnGetAsync(string leagueCode)
    {
        var result = await _footballService.GetStandingsAsync(leagueCode);

        if (result.ErrorMessage != null)
        {
            ErrorMessage = result.ErrorMessage;
        }
        else
        {
            StandingsData = result.Data;
        }
    }
}
