using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Api.Data;
using Pos.Api.Models;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly PosDbContext _db;

        public ProductsController(PosDbContext db)
        {
            _db = db;
        }

        // GET /api/products?includeInactive=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll([FromQuery] bool includeInactive = false)
        {
            var query = _db.Products.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            var products = await query
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Ok(products);
        }

        // GET /api/products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> Get(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // POST /api/products
        [HttpPost]
        public async Task<ActionResult<Product>> Create(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest("Name is required.");

            // default active if not set
            if (!product.IsActive)
                product.IsActive = true;

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        // PUT /api/products/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Product updated)
        {
            if (id != updated.Id)
                return BadRequest("ID mismatch.");

            var existing = await _db.Products.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Sku = updated.Sku;
            existing.Name = updated.Name;
            existing.Price = updated.Price;
            existing.IsActive = updated.IsActive;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/products/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _db.Products.FindAsync(id);
            if (existing == null) return NotFound();

            _db.Products.Remove(existing);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
