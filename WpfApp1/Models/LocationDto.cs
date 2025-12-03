namespace Pos.Desktop.Models
{
    public class LocationDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;   // MAIN, STORE2, etc.
        public string Name { get; set; } = string.Empty;   // "Main Store"
        public string? Address { get; set; }
    }
}
