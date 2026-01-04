using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Api.Data;
using Pos.Api.Models;
using Pos.Api.Models.Dtos;

namespace Pos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly PosDbContext _db;

        public LocationsController(PosDbContext db)
        {
            _db = db;
        }

        // GET /api/locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetAll()
        {
            var locations = await _db.Locations
                .OrderBy(l => l.Name)
                .ToListAsync();

            return Ok(locations);
        }

        // PUT /api/locations/{id}/pos-status
        [HttpPut("{id:int}/pos-status")]
        public async Task<ActionResult<Location>> UpdatePosStatus(
            int id,
            [FromBody] UpdateLocationPosStatusDto dto)
        {
            var location = await _db.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            location.IsPosDisabled = dto.IsPosDisabled;
            await _db.SaveChangesAsync();

            return Ok(location);
        }
    }
}

