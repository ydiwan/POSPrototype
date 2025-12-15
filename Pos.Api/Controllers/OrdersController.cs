using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Api.Data;
using Pos.Api.Models;
using Pos.Api.Models.Dtos;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly PosDbContext _db;

        public OrdersController(PosDbContext db)
        {
            _db = db;
        }

        // POST /api/orders
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
            {
                return BadRequest("Order must contain at least one item.");
            }

            // default to MAIN if nothing provided
            var locationCode = string.IsNullOrWhiteSpace(dto.LocationCode)
                ? "MAIN"
                : dto.LocationCode;

            var location = await _db.Locations
                .FirstOrDefaultAsync(l => l.Code == locationCode);

            if (location == null)
            {
                return BadRequest($"Unknown location code: {locationCode}");
            }

            // load products in one query
            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            decimal subtotal = 0m;

            var order = new Order
            {
                LocationId = location.Id
            };

            foreach (var item in dto.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    return BadRequest($"Invalid product id: {item.ProductId}");
                }

                var lineTotal = product.Price * item.Quantity;
                subtotal += lineTotal;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    LineTotal = lineTotal
                });
            }

            const decimal taxRate = 0.06m;
            order.Subtotal = subtotal;
            order.Tax = Math.Round(subtotal * taxRate, 2);
            order.Total = order.Subtotal + order.Tax;
            order.Status = "paid";

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            var response = new OrderResponse
            {
                Id = order.Id,
                Subtotal = order.Subtotal,
                Tax = order.Tax,
                Total = order.Total,
                Status = order.Status,
                LocationCode = location.Code,
                CreatedAtUtc = order.CreatedAtUtc
            };

            return Ok(response);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderListDto>>> GetOrders(
    DateTime? from,
    DateTime? to,
    string? locationCode)
        {
            var query = _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Location)   // ✅ join Location
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(o => o.CreatedAtUtc >= from.Value);

            if (to.HasValue)
                query = query.Where(o => o.CreatedAtUtc <= to.Value);

            if (!string.IsNullOrWhiteSpace(locationCode) && locationCode != "ALL")
                query = query.Where(o => o.Location.Code == locationCode); // ✅ filter by Location.Code

            var orders = await query
                .OrderByDescending(o => o.CreatedAtUtc)
                .Select(o => new OrderListDto
                {
                    Id = o.Id,
                    CreatedAtUtc = o.CreatedAtUtc,
                    LocationCode = o.Location.Code, // ✅
                    Total = o.Total,
                    ItemCount = o.Items.Sum(i => i.Quantity)
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDetailDto>> GetOrder(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Location) // ✅
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var dto = new OrderDetailDto
            {
                Id = order.Id,
                CreatedAtUtc = order.CreatedAtUtc,
                LocationCode = order.Location.Code, // ✅
                Total = order.Total,
                Items = order.Items.Select(i => new OrderLineDto
                {
                    ProductName = i.Product.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(dto);
        }



    }
}
