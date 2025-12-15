using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pos.Dashboard.Models;

namespace Pos.Dashboard.Pages
{
    public class OrdersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrdersModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<LocationDto> Locations { get; set; } = new();
        public List<OrderListDto> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string LocationCode { get; set; } = "ALL";

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("PosApi");

            // Load locations for dropdown
            var locs = await client.GetFromJsonAsync<List<LocationDto>>("api/locations");
            Locations = locs ?? new();

            // Default date range = last 7 days
            if (!From.HasValue) From = DateTime.UtcNow.Date.AddDays(-7);
            if (!To.HasValue) To = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            var url = $"api/orders?from={Uri.EscapeDataString(From.Value.ToString("o"))}&to={Uri.EscapeDataString(To.Value.ToString("o"))}";

            if (!string.IsNullOrWhiteSpace(LocationCode) && LocationCode != "ALL")
                url += $"&locationCode={Uri.EscapeDataString(LocationCode)}";

            var orders = await client.GetFromJsonAsync<List<OrderListDto>>(url);
            Orders = orders ?? new();
        }
    }
}

