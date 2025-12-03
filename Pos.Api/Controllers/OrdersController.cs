using Microsoft.AspNetCore.Mvc;
using Pos.Api.Models;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        // This simulates an auto-incrementing order ID.
        // In a real app, the database would handle this.
        private static int _nextOrderId = 1;

        // We'll re-use the same hard-coded products used by ProductsController.
        // In a real app, you'd move this to a shared service or database.
        private static readonly List<Product> Products = new()
        {
            new Product { Id = 1, Sku = "COFFEE_SMALL", Name = "Small Coffee", Price = 2.50m },
            new Product { Id = 2, Sku = "COFFEE_LARGE", Name = "Large Coffee", Price = 3.50m },
            new Product { Id = 3, Sku = "PASTRY",       Name = "Pastry",       Price = 4.00m }
        };

        [HttpPost]
        public ActionResult<OrderResponse> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
            {
                return BadRequest("Order must contain at least one item.");
            }

            decimal subtotal = 0m;

            foreach (var item in dto.Items)
            {
                var product = Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    return BadRequest($"Invalid product id: {item.ProductId}");
                }

                subtotal += product.Price * item.Quantity;
            }

            var taxRate = 0.06m;  // 6% tax, just to have something
            var tax = Math.Round(subtotal * taxRate, 2);
            var total = subtotal + tax;

            var orderId = _nextOrderId++;
            var response = new OrderResponse
            {
                Id = orderId,
                Subtotal = subtotal,
                Tax = tax,
                Total = total,
                Status = "paid",                   // we’ll hook real payment in later
                LocationCode = dto.LocationCode,
                CreatedAtUtc = DateTime.UtcNow
            };

            // Log to console so you can see it in the output window.
            Console.WriteLine($"[ORDER] #{response.Id} Location={response.LocationCode} Total={response.Total}");

            return Ok(response);
        }
    }
}
