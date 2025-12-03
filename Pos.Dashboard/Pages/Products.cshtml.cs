using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pos.Dashboard.Models;

namespace Pos.Dashboard.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ProductDto> Products { get; set; } = new();

        // Form model (used for both create and edit)
        [BindProperty]
        public ProductDto FormProduct { get; set; } = new();

        // Used to enter edit mode via query string: /Products?EditId=5
        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("PosApi");

            // include inactive so you can see everything
            var products = await client.GetFromJsonAsync<List<ProductDto>>("api/products?includeInactive=true");
            Products = products ?? new List<ProductDto>();

            // If EditId is set, load that product into the form
            if (EditId.HasValue)
            {
                var product = await client.GetFromJsonAsync<ProductDto>($"api/products/{EditId.Value}");
                if (product != null)
                {
                    FormProduct = product;
                }
            }
        }

        // Create or update
        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("PosApi");

            if (!ModelState.IsValid)
            {
                // reload list and stay on page
                var products = await client.GetFromJsonAsync<List<ProductDto>>("api/products?includeInactive=true");
                Products = products ?? new List<ProductDto>();
                return Page();
            }

            if (FormProduct.Id == 0)
            {
                // New product
                FormProduct.IsActive = true;
                var response = await client.PostAsJsonAsync("api/products", FormProduct);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, $"Failed to create product: {response.StatusCode}");
                    var products = await client.GetFromJsonAsync<List<ProductDto>>("api/products?includeInactive=true");
                    Products = products ?? new List<ProductDto>();
                    return Page();
                }
            }
            else
            {
                // Update existing
                var response = await client.PutAsJsonAsync($"api/products/{FormProduct.Id}", FormProduct);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, $"Failed to update product: {response.StatusCode}");
                    var products = await client.GetFromJsonAsync<List<ProductDto>>("api/products?includeInactive=true");
                    Products = products ?? new List<ProductDto>();
                    return Page();
                }
            }

            return RedirectToPage("/Products");
        }

        // Delete handler
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("PosApi");
            await client.DeleteAsync($"api/products/{id}");
            return RedirectToPage("/Products");
        }

        // Toggle Active/Inactive
        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("PosApi");

            var product = await client.GetFromJsonAsync<ProductDto>($"api/products/{id}");
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                await client.PutAsJsonAsync($"api/products/{id}", product);
            }

            return RedirectToPage("/Products");
        }
    }
}
