using System.Collections.Generic;
using System.Linq;

namespace Pos.Desktop.Models
{
    public class ProductGroup
    {
        public string Name { get; set; } = string.Empty;
        public string? OptionGroup { get; set; }
        public List<Product> Variants { get; set; } = new();

        // For display in the list
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(OptionGroup) || Variants.Count <= 1)
                    return Name;

                return $"{Name} ({OptionGroup})";
            }
        }

        public override string ToString() => DisplayName;
    }
}

