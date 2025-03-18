using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Threading.Tasks;

public class ScorersModel : PageModel
{
    private readonly FootballService _footballService;

    public JsonDocument? ScorersData { get; private set; }
    public string? ErrorMessage { get; private set; }

    public ScorersModel(FootballService footballService)
    {
        _footballService = footballService;
    }

    public async Task OnGetAsync(string leagueCode)
    {
        var result = await _footballService.GetTopScorersAsync(leagueCode);

        if (result.ErrorMessage != null)
        {
            ErrorMessage = result.ErrorMessage;
        }
        else
        {
            ScorersData = result.Data;
        }
    }
}
