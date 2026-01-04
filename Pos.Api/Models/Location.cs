using System.Collections.Generic;

namespace Pos.Api.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;  // e.g. "MAIN", "STORE2"
        public string Name { get; set; } = string.Empty;  // e.g. "Main Street"
        public string? Address { get; set; }
        public bool IsPosDisabled { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

