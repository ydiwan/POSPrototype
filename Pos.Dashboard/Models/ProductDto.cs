namespace Pos.Dashboard.Models
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string? OptionGroup { get; set; }   // e.g. "Size"
        public string? OptionValue { get; set; }   // e.g. "Small"

        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

