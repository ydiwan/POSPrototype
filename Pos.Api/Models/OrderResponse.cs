using System;

namespace Pos.Api.Models
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = "paid";
        public string LocationCode { get; set; } = "MAIN";
        public DateTime CreatedAtUtc { get; set; }
    }
}
