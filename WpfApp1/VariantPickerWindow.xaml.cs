using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Pos.Desktop.Models;

namespace Pos.Desktop
{
    public partial class VariantPickerWindow : Window
    {
        public Product? SelectedProduct { get; private set; }

        public VariantPickerWindow(IEnumerable<Product> variants, string name, string? optionGroup)
        {
            InitializeComponent();

            Title = $"Choose {optionGroup ?? "Option"} for {name}";

            // show "Small - $2.50" etc.
            VariantsList.ItemsSource = variants
                .Select(v => new VariantDisplay(v))
                .ToList();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (VariantsList.SelectedItem is VariantDisplay vd)
            {
                SelectedProduct = vd.Product;
                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }

            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private class VariantDisplay
        {
            public VariantDisplay(Product product)
            {
                Product = product;
                Name = $"{product.OptionValue ?? ""} - {product.Price:C}";
            }

            public Product Product { get; }
            public string Name { get; }
        }
    }
}
