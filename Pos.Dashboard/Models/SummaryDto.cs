using System;

namespace Pos.Dashboard.Models
{
    public class SummaryDto
    {
        public DateTime Date { get; set; }
        public string LocationCode { get; set; } = "ALL";
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
        public decimal AvgOrder { get; set; }
    }
}
