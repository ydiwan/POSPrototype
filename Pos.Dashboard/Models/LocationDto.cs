namespace Pos.Dashboard.Models
{
    public class LocationDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool IsPosDisabled { get; set; }
    }
}
