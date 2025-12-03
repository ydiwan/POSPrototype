using System.Collections.Generic;

namespace Pos.Api.Models
{
    public class CreateOrderDto
    {
        public string LocationCode { get; set; } = "MAIN";
        public List<OrderItemDto> Items { get; set; } = new();

        public class OrderItemDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
