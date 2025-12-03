using Microsoft.AspNetCore.Mvc;
using Pos.Api.Models;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // For now, just return a hard-coded list.
        // Later we'll swap this to read from a database.
        private static readonly List<Product> Products = new()
        {
            new Product { Id = 1, Sku = "COFFEE_SMALL", Name = "Small Coffee", Price = 2.50m },
            new Product { Id = 2, Sku = "COFFEE_LARGE", Name = "Large Coffee", Price = 3.50m },
            new Product { Id = 3, Sku = "PASTRY",       Name = "Pastry",       Price = 4.00m }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            return Ok(Products);
        }
    }
}
