using Pos.Desktop.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pos.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new();
        private ObservableCollection<ProductGroup> _productGroups = new();
        private ObservableCollection<CartItem> _cartItems = new();

        private ObservableCollection<LocationDto> _locations = new();
        private string _currentLocationCode = "MAIN";  // default

        private decimal _subtotal;
        private decimal _tax;
        private decimal _total;
        private DispatcherTimer? _connectionTimer;
        private bool _isCheckingConnection;

        public MainWindow()
        {
            InitializeComponent();

            CartGrid.ItemsSource = _cartItems;

            _httpClient.BaseAddress = new Uri("http://localhost:5171/"); // your API HTTP URL

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLocationsAsync();
            await LoadProductsAsync();
            UpdateTotals();
            StartConnectionMonitor();
        }
        private async Task LoadLocationsAsync()
        {
            try
            {
                var locations = await _httpClient.GetFromJsonAsync<LocationDto[]>("api/locations");
                UpdateConnectionStatus(true);
                if (locations != null && locations.Length > 0)
                {
                    _locations = new ObservableCollection<LocationDto>(locations);
                    LocationCombo.ItemsSource = _locations;

                    // Pick saved/default location or fallback to first enabled location
                    var chosen = _locations.FirstOrDefault(l => l.Code == _currentLocationCode && !l.IsPosDisabled)
                                 ?? _locations.FirstOrDefault(l => !l.IsPosDisabled)
                                 ?? _locations[0];

                    LocationCombo.SelectedItem = chosen;
                    UpdateLocationStatus(chosen);
                }
                else
                {
                    LocationStatusText.Text = "No locations found";
                    CompleteSaleButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load locations: {ex.Message}");
                LocationStatusText.Text = "Location: (error)";
                CompleteSaleButton.IsEnabled = false;
                UpdateConnectionStatus(false);
            }
        }

        private void LocationCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocationCombo.SelectedItem is LocationDto loc)
            {
                UpdateLocationStatus(loc);
                // later we can save this to a local config file
            }
        }

        private void UpdateLocationStatus(LocationDto loc)
        {
            _currentLocationCode = loc.Code;

            var status = $"Active location: {_currentLocationCode}";
            if (loc.IsPosDisabled)
            {
                status += " (POS disabled)";
            }

            LocationStatusText.Text = status;
            CompleteSaleButton.IsEnabled = !loc.IsPosDisabled;
        }

        private void UpdateConnectionStatus(bool isOnline)
        {
            ConnectionStatusDot.Fill = isOnline ? Brushes.Green : Brushes.Red;
            ConnectionStatusDot.Stroke = isOnline ? Brushes.DarkGreen : Brushes.DarkRed;
            ConnectionStatusText.Text = isOnline ? "Online" : "Offline";
        }

        private void StartConnectionMonitor()
        {
            if (_connectionTimer != null)
            {
                return;
            }

            _connectionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _connectionTimer.Tick += async (_, _) => await CheckConnectionAsync();
            _connectionTimer.Start();
        }

        private async Task CheckConnectionAsync()
        {
            if (_isCheckingConnection)
            {
                return;
            }

            _isCheckingConnection = true;
            try
            {
                var response = await _httpClient.GetAsync(
                    "api/locations",
                    HttpCompletionOption.ResponseHeadersRead);
                UpdateConnectionStatus(response.IsSuccessStatusCode);
            }
            catch (HttpRequestException)
            {
                UpdateConnectionStatus(false);
            }
            finally
            {
                _isCheckingConnection = false;
            }
        }


        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<Product>>("api/products");
                if (products == null) return;

                // build groups by Name + OptionGroup
                var groups = products
                    .GroupBy(p => new { p.Name, p.OptionGroup })
                    .Select(g => new ProductGroup
                    {
                        Name = g.Key.Name,
                        OptionGroup = g.Key.OptionGroup,
                        Variants = g.ToList()
                    })
                    .OrderBy(g => g.Name)
                    .ToList();

                _productGroups = new ObservableCollection<ProductGroup>(groups);
                ProductsList.ItemsSource = _productGroups;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load products: {ex.Message}");
                if (ex is HttpRequestException)
                {
                    UpdateConnectionStatus(false);
                }
            }
        }


        private void ProductsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProductsList.SelectedItem is not ProductGroup group)
                return;

            Product? chosen;

            if (group.Variants.Count == 1)
            {
                // simple case: only one variant
                chosen = group.Variants[0];
            }
            else
            {
                // show popup to pick variant
                var picker = new VariantPickerWindow(group.Variants, group.Name, group.OptionGroup);
                picker.Owner = this;
                var result = picker.ShowDialog();
                if (result != true || picker.SelectedProduct == null)
                    return;

                chosen = picker.SelectedProduct;
            }

            // add chosen product to cart
            var existing = _cartItems.FirstOrDefault(c => c.Product.Id == chosen.Id);
            if (existing == null)
            {
                // ✅ use your CartItem(Product, int) constructor
                _cartItems.Add(new CartItem(chosen, 1));
            }
            else
            {
                // just bump quantity; LineTotal is computed property
                existing.Quantity++;
            }

            CartGrid.Items.Refresh();
            UpdateTotals();
        }



        private void AddProductToCart(Product product)
        {
            var existing = _cartItems.FirstOrDefault(c => c.Product.Id == product.Id);
            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                _cartItems.Add(new CartItem(product));
            }

            CartGrid.Items.Refresh();
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            _subtotal = _cartItems.Sum(c => c.LineTotal);
            _tax = Math.Round(_subtotal * 0.06m, 2); // must match backend
            _total = _subtotal + _tax;

            SubtotalText.Text = $"Subtotal: {_subtotal:C}";
            TaxText.Text = $"Tax (6%): {_tax:C}";
            TotalText.Text = $"Total: {_total:C}";
        }

        private async void CompleteSale_Click(object sender, RoutedEventArgs e)
        {
            if (!_cartItems.Any())
            {
                MessageBox.Show("Cart is empty.");
                return;
            }
            if (LocationCombo.SelectedItem is LocationDto selectedLoc && selectedLoc.IsPosDisabled)
            {
                MessageBox.Show("POS is disabled for this location.");
                return;
            }

            try
            {
                var payload = new
                {
                    LocationCode = _currentLocationCode,
                    Items = _cartItems.Select(c => new { ProductId = c.Product.Id, Quantity = c.Quantity }).ToList()
                };

                var response = await _httpClient.PostAsJsonAsync("api/orders", payload);
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    MessageBox.Show("POS is disabled for this location.");
                    return;
                }
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OrderResponse>();

                MessageBox.Show($"Sale complete! Order #{result?.Id} Total: {result?.Total:C}");

                _cartItems.Clear();
                CartGrid.Items.Refresh();
                UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to complete sale: {ex.Message}");
                if (ex is HttpRequestException)
                {
                    UpdateConnectionStatus(false);
                }
            }
        }

        private class OrderResponse
        {
            public int Id { get; set; }
            public decimal Total { get; set; }
            public string Status { get; set; } = "";
            public DateTime CreatedAtUtc { get; set; }
        }
    }
}
