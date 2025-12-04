namespace Pos.Api.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Option group/value for variants, e.g. "Size" / "Small"
        public string? OptionGroup { get; set; }   // e.g. "Size"
        public string? OptionValue { get; set; }   // e.g. "Small"

        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
