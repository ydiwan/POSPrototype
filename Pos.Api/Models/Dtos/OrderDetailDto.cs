namespace Pos.Api.Models.Dtos
{
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string LocationCode { get; set; } = "";
        public decimal Total { get; set; }

        public List<OrderLineDto> Items { get; set; } = new();
    }

    public class OrderLineDto
    {
        public string ProductName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}

