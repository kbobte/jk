using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    private readonly FootballService _footballService;
    public JsonDocument? CompetitionsData { get; private set; }
    public string? ErrorMessage { get; private set; }

    public IndexModel(FootballService footballService)
    {
        _footballService = footballService;
    }

    public async Task OnGetAsync()
    {
        var result = await _footballService.GetCompetitionsAsync();

        if (result.ErrorMessage != null)
        {
            ErrorMessage = result.ErrorMessage;
        }
        else
        {
            CompetitionsData = result.Data;
        }
    }
}
