using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pos.Dashboard.Models;

namespace Pos.Dashboard.Pages
{
    public class OrderDetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderDetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public OrderDetailDto? Order { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("PosApi");
            Order = await client.GetFromJsonAsync<OrderDetailDto>($"api/orders/{Id}");

            if (Order == null)
                return RedirectToPage("/Orders");

            return Page();
        }
    }
}
