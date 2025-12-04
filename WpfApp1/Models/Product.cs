namespace Pos.Desktop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string? OptionGroup { get; set; }
        public string? OptionValue { get; set; }

        public decimal Price { get; set; }
    }
}

