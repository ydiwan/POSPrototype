namespace Pos.Api.Models.Dtos
{
    public class OrderListDto
    {
        public int Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string LocationCode { get; set; } = "";
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
    }
}

