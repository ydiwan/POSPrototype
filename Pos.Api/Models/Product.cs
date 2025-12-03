namespace Pos.Api.Models
{
    public class Product
    {
        public int Id { get; set; }          // simple integer id
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}

