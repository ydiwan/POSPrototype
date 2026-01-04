using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pos.Dashboard.Models;

namespace Pos.Dashboard.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<LocationDto> Locations { get; set; } = new();
        public SummaryDto? Summary { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Location { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("PosApi");

            var locs = await client.GetFromJsonAsync<List<LocationDto>>("api/locations");
            Locations = locs ?? new List<LocationDto>();

            string summaryUrl = "api/stats/summary";

            string? code = string.IsNullOrWhiteSpace(Location) || Location == "ALL"
                ? null
                : Location;

            if (!string.IsNullOrWhiteSpace(code))
            {
                summaryUrl += $"?locationCode={code}";
            }

            var summary = await client.GetFromJsonAsync<SummaryDto>(summaryUrl);
            Summary = summary ?? new SummaryDto
            {
                Date = DateTime.UtcNow.Date,
                LocationCode = code ?? "ALL"
            };

            if (string.IsNullOrWhiteSpace(Location))
            {
                Location = "ALL";
            }
        }

        public async Task<IActionResult> OnPostTogglePosAsync(int id, bool disable, string? location)
        {
            Location = location;

            var client = _httpClientFactory.CreateClient("PosApi");
            var response = await client.PutAsJsonAsync(
                $"api/locations/{id}/pos-status",
                new { IsPosDisabled = disable });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to update POS status.");
            }

            await OnGetAsync();
            return Page();
        }
    }
}
