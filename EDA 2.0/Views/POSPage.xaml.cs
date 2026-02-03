using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.CustomerFeature.Commands.CreateCustomerCommand;
using EDA.APPLICATION.Features.CustomerFeature.Queries;
using EDA.APPLICATION.Features.DiscountFeature.Queries;
using EDA.APPLICATION.Features.InvoiceFeature.Commands.CreateInvoiceCommand;
using EDA.APPLICATION.Features.PaymentTypeFeature.Queries;
using EDA.APPLICATION.Features.ProductFeature.Commands.CreateProductCommand;
using EDA.APPLICATION.Features.ProductFeature.Queries;
using EDA.APPLICATION.Features.FamilyFeature.Queries;
using EDA.APPLICATION.Features.TaxFeature.Queries;
using EDA.APPLICATION.Interfaces;
using EDA.APPLICATION.Specifications.CaiSpecification;
using EDA.DOMAIN.Entities;
using Ardalis.Specification.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EDA.INFRAESTRUCTURE;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EDA_2._0.Views
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;
        private decimal _unitPrice;

        public Product Product { get; set; } = null!;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value && value >= 1)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice != value && value >= 0)
                {
                    _unitPrice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        public decimal Subtotal => Quantity * UnitPrice;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PaymentItem
    {
        public PaymentType PaymentType { get; set; } = null!;
        public decimal Amount { get; set; }
    }

    public class SaleSession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int? DbId { get; set; }
        public string DisplayName { get; set; } = "Venta 1";
        public ObservableCollection<CartItem> CartItems { get; set; } = new();
        public ObservableCollection<PaymentItem> PaymentItems { get; set; } = new();
        public Customer? SelectedCustomer { get; set; }
        public Discount? SelectedDiscount { get; set; }
    }

    public class PendingSaleData
    {
        public List<PendingSaleItemData> Items { get; set; } = new();
        public List<PendingSalePaymentData> Payments { get; set; } = new();
        public int? CustomerId { get; set; }
        public int? DiscountId { get; set; }
    }

    public class PendingSaleItemData
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PendingSalePaymentData
    {
        public int PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
    }

    public sealed partial class POSPage : Page
    {
        private readonly IMediator _mediator;
        private readonly DatabaseContext _dbContext;

        private List<Product> _allProducts = new();
        private List<Customer> _customers = new();
        private List<Discount> _discounts = new();
        private List<PaymentType> _paymentTypes = new();
        private List<Family> _families = new();
        private List<Tax> _taxes = new();
        private Cai? _activeCai;
        private Discount? _selectedDiscount;
        private Customer? _selectedCustomer;
        private Family? _selectedFamily;

        private ObservableCollection<CartItem> _cartItems = new();
        private ObservableCollection<PaymentItem> _paymentItems = new();

        private List<SaleSession> _saleSessions = new();
        private SaleSession _currentSession = null!;
        private int _saleCounter = 0;

        public POSPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
            _dbContext = App.Services.GetRequiredService<DatabaseContext>();

            CartListView.ItemsSource = _cartItems;
            PaymentsListView.ItemsSource = _paymentItems;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInitialData();
            SetupCashierInfo();
            await LoadPendingSalesFromDb();
            if (_saleSessions.Count == 0)
            {
                CreateNewSaleSession();
            }
            else
            {
                LoadSession(_saleSessions[0]);
            }
            UpdateTotals();
        }

        private async Task LoadInitialData()
        {
            try
            {
                // Cargar productos
                var productsResult = await _mediator.Send(new GetAllProductsQuery { GetAll = true });
                if (productsResult.Succeeded && productsResult.Data != null)
                {
                    _allProducts = productsResult.Data.Items.ToList();
                    ProductsItemsControl.ItemsSource = _allProducts;
                }

                // Cargar clientes
                var customersResult = await _mediator.Send(new GetAllCustomersQuery { GetAll = true });
                if (customersResult.Succeeded && customersResult.Data != null)
                {
                    _customers = customersResult.Data.Items.ToList();

                    // Seleccionar Consumidor Final por defecto (ID=1)
                    var defaultCustomer = _customers.FirstOrDefault(c => c.Id == 1);
                    SetSelectedCustomer(defaultCustomer);
                }

                // Cargar descuentos
                var discountsResult = await _mediator.Send(new GetAllDiscountsQuery { GetAll = true });
                if (discountsResult.Succeeded && discountsResult.Data != null)
                {
                    _discounts = discountsResult.Data.Items.ToList();
                    DiscountButtonsControl.ItemsSource = _discounts;
                }

                // Cargar tipos de pago
                var paymentTypesResult = await _mediator.Send(new GetAllPaymentTypesQuery { GetAll = true });
                if (paymentTypesResult.Succeeded && paymentTypesResult.Data != null)
                {
                    _paymentTypes = paymentTypesResult.Data.Items.ToList();
                    PaymentTypeComboBox.ItemsSource = _paymentTypes;
                }

                // Cargar familias (para filtro y crear productos)
                var familiesResult = await _mediator.Send(new GetAllFamiliesQuery { GetAll = true });
                if (familiesResult.Succeeded && familiesResult.Data != null)
                {
                    _families = familiesResult.Data.Items.ToList();
                    BuildFamilyButtons();
                }

                var taxesResult = await _mediator.Send(new GetAllTaxesQuery { GetAll = true });
                if (taxesResult.Succeeded && taxesResult.Data != null)
                {
                    _taxes = taxesResult.Data.Items.ToList();
                }

                // Cargar CAI activo
                await LoadActiveCai();
            }
            catch (Exception ex)
            {
                await ShowError($"Error al cargar datos: {ex.Message}");
            }
        }

        private async Task LoadActiveCai()
        {
            try
            {
                var spec = new GetActiveCaiSpecification();
                _activeCai = await _dbContext.Cais.WithSpecification(spec).FirstOrDefaultAsync();

                if (_activeCai != null)
                {
                    CaiInfoText.Text = $"{_activeCai.Name} ({_activeCai.CurrentCorrelative})";
                }
                else
                {
                    CaiInfoText.Text = "Sin CAI activo";
                }
            }
            catch (Exception)
            {
                CaiInfoText.Text = "Error al cargar CAI";
            }
        }

        private void SetupCashierInfo()
        {
            var currentUser = App.CurrentUser;
            if (currentUser != null)
            {
                CashierInfoText.Text = $"{currentUser.Name} {currentUser.LastName}";
            }
            else
            {
                CashierInfoText.Text = "No identificado";
            }
        }

        #region Product Search and Selection

        private void ProductSearchTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ApplyProductFilters();
            }
        }

        private void BuildFamilyButtons()
        {
            FamiliesPanel.Children.Clear();

            // Contar productos totales
            var totalProductCount = _allProducts.Count;

            // Botón "Todos"
            var allButton = new Button
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Todos",
                            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = $"Productos: {totalProductCount}",
                            FontSize = 11,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                },
                MinWidth = 100,
                Height = 50,
                Padding = new Thickness(12, 6, 12, 6),
                Style = (Style)Application.Current.Resources["AccentButtonStyle"]
            };
            allButton.Click += (s, e) =>
            {
                _selectedFamily = null;
                UpdateFamilyButtonStyles();
                ApplyProductFilters();
            };
            FamiliesPanel.Children.Add(allButton);

            // Filtrar familias que tienen productos y crear botones
            var familiesWithProducts = _families
                .Select(f => new { Family = f, ProductCount = _allProducts.Count(p => p.FamilyId == f.Id) })
                .Where(x => x.ProductCount > 0)
                .ToList();

            foreach (var item in familiesWithProducts)
            {
                var family = item.Family;
                var productCount = item.ProductCount;

                var button = new Button
                {
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = family.Name,
                                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                                HorizontalAlignment = HorizontalAlignment.Center
                            },
                            new TextBlock
                            {
                                Text = $"Productos: {productCount}",
                                FontSize = 11,
                                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }
                    },
                    Tag = family,
                    MinWidth = 100,
                    Height = 50,
                    Padding = new Thickness(12, 6, 12, 6)
                };
                button.Click += (s, e) =>
                {
                    _selectedFamily = family;
                    UpdateFamilyButtonStyles();
                    ApplyProductFilters();
                };
                FamiliesPanel.Children.Add(button);
            }
        }

        private void UpdateFamilyButtonStyles()
        {
            foreach (var child in FamiliesPanel.Children)
            {
                if (child is Button button)
                {
                    var isSelected = false;

                    if (button.Tag is Family family)
                    {
                        isSelected = _selectedFamily != null && _selectedFamily.Id == family.Id;
                    }
                    else
                    {
                        // Botón "Todos"
                        isSelected = _selectedFamily == null;
                    }

                    button.Style = isSelected
                        ? (Style)Application.Current.Resources["AccentButtonStyle"]
                        : (Style)Application.Current.Resources["DefaultButtonStyle"];

                    // Actualizar color del texto secundario según selección
                    if (button.Content is StackPanel stackPanel && stackPanel.Children.Count >= 2)
                    {
                        if (stackPanel.Children[1] is TextBlock countText)
                        {
                            countText.Foreground = isSelected
                                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
                        }
                    }
                }
            }
        }

        private void ApplyProductFilters()
        {
            var searchTerm = ProductSearchTextBox.Text?.Trim().ToLower() ?? "";

            IEnumerable<Product> filtered = _allProducts;

            // Filtrar por familia si hay una seleccionada
            if (_selectedFamily != null)
            {
                filtered = filtered.Where(p => p.FamilyId == _selectedFamily.Id);
            }

            // Filtrar por término de búsqueda
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(searchTerm)));
            }

            ProductsItemsControl.ItemsSource = filtered.ToList();
        }

        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                AddProductToCart(product);
            }
        }

        private async void AddProductToCart(Product product)
        {
            var existingItem = _cartItems.FirstOrDefault(c => c.Product.Id == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var newItem = new CartItem
                {
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price
                };

                // Suscribir a cambios para actualizar totales
                newItem.PropertyChanged += CartItem_PropertyChanged;

                _cartItems.Add(newItem);
            }

            UpdateTotals();

            // Alerta de stock bajo
            if (product.MinStock > 0 && product.Stock <= product.MinStock)
            {
                var dialog = new ContentDialog
                {
                    Title = "Stock Bajo",
                    Content = new StackPanel
                    {
                        Spacing = 12,
                        Children =
                        {
                            new FontIcon
                            {
                                Glyph = "\uE7BA",
                                FontSize = 48,
                                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange),
                                HorizontalAlignment = HorizontalAlignment.Center
                            },
                            new TextBlock
                            {
                                Text = $"El producto \"{product.Name}\" tiene stock bajo.\nStock actual: {product.Stock} | Mínimo: {product.MinStock}",
                                TextWrapping = TextWrapping.Wrap,
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }
                    },
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Quantity) ||
                e.PropertyName == nameof(CartItem.UnitPrice) ||
                e.PropertyName == nameof(CartItem.Subtotal))
            {
                UpdateTotals();
            }
        }

        #endregion

        #region Cart Management

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CartItem item)
            {
                item.Quantity++;
                UpdateTotals();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CartItem item)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    UpdateTotals();
                }
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CartItem item)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
                _cartItems.Remove(item);
                UpdateTotals();
            }
        }

        #endregion

        #region Editable Cart Fields

        private void QuantityNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is NumberBox numberBox && numberBox.Tag is CartItem item)
            {
                ValidateAndUpdateQuantity(numberBox, item);
            }
        }

        private void QuantityNumberBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (sender is NumberBox numberBox && numberBox.Tag is CartItem item)
                {
                    ValidateAndUpdateQuantity(numberBox, item);
                    CartListView.Focus(FocusState.Programmatic);
                }
            }
        }

        private void ValidateAndUpdateQuantity(NumberBox numberBox, CartItem item)
        {
            if (double.IsNaN(numberBox.Value))
            {
                numberBox.Value = item.Quantity;
                return;
            }

            var newQuantity = (int)Math.Round(numberBox.Value);
            if (newQuantity < 1)
            {
                newQuantity = 1;
                numberBox.Value = newQuantity;
            }

            if (item.Quantity != newQuantity)
            {
                item.Quantity = newQuantity;
                UpdateTotals();
            }
        }

        private void PriceNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is NumberBox numberBox && numberBox.Tag is CartItem item)
            {
                ValidateAndUpdatePrice(numberBox, item);
            }
        }

        private void PriceNumberBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (sender is NumberBox numberBox && numberBox.Tag is CartItem item)
                {
                    ValidateAndUpdatePrice(numberBox, item);
                    CartListView.Focus(FocusState.Programmatic);
                }
            }
        }

        private void ValidateAndUpdatePrice(NumberBox numberBox, CartItem item)
        {
            if (double.IsNaN(numberBox.Value))
            {
                numberBox.Value = (double)item.UnitPrice;
                return;
            }

            var newPrice = Math.Round((decimal)numberBox.Value, 2);
            if (newPrice < 0)
            {
                newPrice = 0;
                numberBox.Value = 0;
            }

            if (item.UnitPrice != newPrice)
            {
                item.UnitPrice = newPrice;
                UpdateTotals();
            }
        }

        #endregion

        #region Discount Management

        private void DiscountButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Discount discount)
            {
                _selectedDiscount = discount;
                SelectedDiscountText.Text = $"{discount.Percentage}% {discount.Name}";
                SelectedDiscountPanel.Visibility = Visibility.Visible;
                UpdateTotals();
            }
        }

        private void RemoveDiscount_Click(object sender, RoutedEventArgs e)
        {
            _selectedDiscount = null;
            SelectedDiscountPanel.Visibility = Visibility.Collapsed;
            UpdateTotals();
        }

        #endregion

        #region Payment Management

        private void AddPayment_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentTypeComboBox.SelectedItem is not PaymentType paymentType)
            {
                return;
            }

            if (!decimal.TryParse(PaymentAmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                return;
            }

            _paymentItems.Add(new PaymentItem
            {
                PaymentType = paymentType,
                Amount = amount
            });

            // Limpiar campos
            PaymentAmountTextBox.Text = string.Empty;
            PaymentTypeComboBox.SelectedIndex = -1;

            UpdateTotals();
        }

        private void RemovePayment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PaymentItem item)
            {
                _paymentItems.Remove(item);
                UpdateTotals();
            }
        }

        #endregion

        #region Totals Calculation

        private void UpdateTotals()
        {
            // El precio ingresado ya incluye impuesto, hay que desglosarlo
            decimal totalBruto = _cartItems.Sum(c => c.Subtotal);

            // Calcular descuento sobre el precio con impuesto incluido
            decimal discountPercentage = _selectedDiscount?.Percentage ?? 0m;
            decimal discountAmount = totalBruto * (discountPercentage / 100m);

            // Extraer impuesto del precio (el precio ya incluye impuesto)
            decimal taxAmount = 0;
            foreach (var item in _cartItems)
            {
                if (item.Product.Tax != null)
                {
                    var itemTotal = item.Subtotal;
                    var itemAfterDiscount = itemTotal - (itemTotal * discountPercentage / 100m);
                    var taxRate = item.Product.Tax.Percentage / 100m;
                    taxAmount += itemAfterDiscount * (taxRate / (1m + taxRate));
                }
            }

            // Subtotal = total bruto - descuento - impuesto (precio sin impuesto)
            decimal subtotal = totalBruto - discountAmount - taxAmount;
            decimal total = totalBruto - discountAmount;

            // Calcular pagos
            decimal totalPaid = _paymentItems.Sum(p => p.Amount);
            decimal pending = total - totalPaid;

            // Actualizar UI
            SubtotalText.Text = $"L. {subtotal:N2}";
            DiscountAmountText.Text = $"- L. {discountAmount:N2}";
            TaxAmountText.Text = $"L. {taxAmount:N2}";
            TotalText.Text = $"L. {total:N2}";
            TotalPaidText.Text = $"L. {totalPaid:N2}";

            // Manejar pendiente vs cambio
            if (pending > 0)
            {
                // Falta por pagar
                PendingLabelText.Text = "Pendiente";
                PendingAmountText.Text = $"L. {pending:N2}";
                PendingAmountText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                ChangeAlertPanel.Visibility = Visibility.Collapsed;
            }
            else if (pending < 0)
            {
                // Hay cambio que dar
                decimal change = Math.Abs(pending);
                PendingLabelText.Text = "Cambio";
                PendingAmountText.Text = $"L. {change:N2}";
                PendingAmountText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);

                // Mostrar alerta de cambio grande y visible
                ChangeAmountText.Text = $"L. {change:N2}";
                ChangeAlertPanel.Visibility = Visibility.Visible;
            }
            else
            {
                // Pago exacto
                PendingLabelText.Text = "Pendiente";
                PendingAmountText.Text = "L. 0.00";
                PendingAmountText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
                ChangeAlertPanel.Visibility = Visibility.Collapsed;
            }

            // Habilitar/deshabilitar botón facturar
            FacturarButton.IsEnabled = _cartItems.Count > 0 && totalPaid >= total && total > 0;
        }

        #endregion

        #region Quick Create Dialogs

        private async void NewProductButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCreateProductDialog();
        }

        private async void NewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCreateCustomerDialog();
        }

        private async Task ShowCreateProductDialog()
        {
            var nameTextBox = new TextBox
            {
                Header = "Nombre del producto *",
                PlaceholderText = "Ingrese el nombre",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var priceNumberBox = new NumberBox
            {
                Header = "Precio *",
                PlaceholderText = "0.00",
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var familyComboBox = new ComboBox
            {
                Header = "Familia *",
                PlaceholderText = "Seleccione familia",
                ItemsSource = _families,
                DisplayMemberPath = "Name",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var taxComboBox = new ComboBox
            {
                Header = "Impuesto *",
                PlaceholderText = "Seleccione impuesto",
                ItemsSource = _taxes,
                DisplayMemberPath = "Name",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var content = new StackPanel
            {
                Width = 350,
                Children = { nameTextBox, priceNumberBox, familyComboBox, taxComboBox }
            };

            var dialog = new ContentDialog
            {
                Title = "Nuevo Producto",
                Content = content,
                PrimaryButtonText = "Crear",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var name = nameTextBox.Text?.Trim();
                var price = double.IsNaN(priceNumberBox.Value) ? 0 : (decimal)priceNumberBox.Value;
                var family = familyComboBox.SelectedItem as Family;
                var tax = taxComboBox.SelectedItem as Tax;

                if (string.IsNullOrEmpty(name) || family == null || tax == null)
                {
                    await ShowError("Complete todos los campos requeridos.");
                    return;
                }

                try
                {
                    var command = new CreateProductCommand
                    {
                        Name = name,
                        Price = price,
                        Stock = 0,
                        FamilyId = family.Id,
                        TaxId = tax.Id
                    };

                    var createResult = await _mediator.Send(command);

                    if (createResult.Succeeded && createResult.Data != null)
                    {
                        _allProducts.Add(createResult.Data);
                        ProductsItemsControl.ItemsSource = null;
                        ProductsItemsControl.ItemsSource = _allProducts;
                        await ShowSuccess("Producto creado exitosamente.");
                    }
                    else
                    {
                        await ShowError(createResult.Message ?? "Error al crear producto.");
                    }
                }
                catch (EDA.APPLICATION.Exceptions.ValidationException vex)
                {
                    await ShowError(string.Join("\n", vex.Errors));
                }
                catch (Exception ex)
                {
                    await ShowError($"Error: {ex.Message}");
                }
            }
        }

        private async Task ShowCreateCustomerDialog()
        {
            var nameTextBox = new TextBox
            {
                Header = "Nombre *",
                PlaceholderText = "Ingrese el nombre del cliente",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var rtnTextBox = new TextBox
            {
                Header = "RTN",
                PlaceholderText = "Ingrese el RTN (opcional)",
                Margin = new Thickness(0, 0, 0, 12),
                MaxLength = 20
            };

            var companyTextBox = new TextBox
            {
                Header = "Empresa",
                PlaceholderText = "Nombre de la empresa (opcional)",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var phoneTextBox = new TextBox
            {
                Header = "Telefono",
                PlaceholderText = "Numero de telefono (opcional)",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var content = new StackPanel
            {
                Width = 350,
                Children = { nameTextBox, rtnTextBox, companyTextBox, phoneTextBox }
            };

            var dialog = new ContentDialog
            {
                Title = "Nuevo Cliente",
                Content = content,
                PrimaryButtonText = "Crear",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var name = nameTextBox.Text?.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    await ShowError("El nombre es requerido.");
                    return;
                }

                try
                {
                    var command = new CreateCustomerCommand
                    {
                        Name = name,
                        RTN = string.IsNullOrWhiteSpace(rtnTextBox.Text) ? null : rtnTextBox.Text.Trim(),
                        Company = string.IsNullOrWhiteSpace(companyTextBox.Text) ? null : companyTextBox.Text.Trim(),
                        PhoneNumber = string.IsNullOrWhiteSpace(phoneTextBox.Text) ? null : phoneTextBox.Text.Trim()
                    };

                    var createResult = await _mediator.Send(command);

                    if (createResult.Succeeded && createResult.Data != null)
                    {
                        _customers.Add(createResult.Data);
                        SetSelectedCustomer(createResult.Data);
                        await ShowSuccess("Cliente creado exitosamente.");
                    }
                    else
                    {
                        await ShowError(createResult.Message ?? "Error al crear cliente.");
                    }
                }
                catch (EDA.APPLICATION.Exceptions.ValidationException vex)
                {
                    await ShowError(string.Join("\n", vex.Errors));
                }
                catch (Exception ex)
                {
                    await ShowError($"Error: {ex.Message}");
                }
            }
        }

        #endregion

        #region Customer Search

        private async void CustomerSelectButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCustomerSearchDialog();
        }

        private async Task ShowCustomerSearchDialog()
        {
            var searchTextBox = new TextBox
            {
                PlaceholderText = "Buscar por nombre, RTN o empresa...",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var resultsListView = new ListView
            {
                MaxHeight = 300,
                SelectionMode = ListViewSelectionMode.Single
            };

            // Cargar resultados iniciales
            resultsListView.ItemsSource = _customers.Take(50).ToList();

            // Búsqueda en tiempo real
            searchTextBox.TextChanged += (s, args) =>
            {
                var term = searchTextBox.Text?.Trim().ToLower() ?? "";
                if (string.IsNullOrEmpty(term))
                {
                    resultsListView.ItemsSource = _customers.Take(50).ToList();
                }
                else
                {
                    resultsListView.ItemsSource = _customers
                        .Where(c => c.Name.ToLower().Contains(term) ||
                                   (c.RTN != null && c.RTN.ToLower().Contains(term)) ||
                                   (c.Company != null && c.Company.ToLower().Contains(term)))
                        .Take(50)
                        .ToList();
                }
            };

            // Template para mostrar nombre y RTN
            resultsListView.ItemTemplate = (DataTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"
                <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <StackPanel Padding='8,4'>
                        <TextBlock Text='{Binding Name}' FontWeight='SemiBold'/>
                        <TextBlock Text='{Binding RTN}' FontSize='12' Foreground='Gray'/>
                    </StackPanel>
                </DataTemplate>");

            var content = new StackPanel
            {
                Width = 400,
                Children = { searchTextBox, resultsListView }
            };

            var dialog = new ContentDialog
            {
                Title = "Seleccionar Cliente",
                Content = content,
                PrimaryButtonText = "Seleccionar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
                IsPrimaryButtonEnabled = false
            };

            resultsListView.SelectionChanged += (s, args) =>
            {
                dialog.IsPrimaryButtonEnabled = resultsListView.SelectedItem != null;
            };

            resultsListView.DoubleTapped += (s, args) =>
            {
                if (resultsListView.SelectedItem is Customer customer)
                {
                    SetSelectedCustomer(customer);
                    dialog.Hide();
                }
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && resultsListView.SelectedItem is Customer selectedCustomer)
            {
                SetSelectedCustomer(selectedCustomer);
            }
        }

        private void SetSelectedCustomer(Customer? customer, bool updateTabName = true)
        {
            _selectedCustomer = customer;
            CustomerSelectButton.Content = customer?.Name ?? "Seleccionar cliente...";

            if (updateTabName && _currentSession != null)
            {
                _currentSession.DisplayName = customer != null && customer.Id != 1
                    ? customer.Name
                    : $"Venta {_saleSessions.IndexOf(_currentSession) + 1}";
                BuildSaleTabButtons();
            }
        }

        #endregion

        #region Sale Sessions

        private void SaveCurrentSessionState()
        {
            if (_currentSession == null) return;
            _currentSession.CartItems = _cartItems;
            _currentSession.PaymentItems = _paymentItems;
            _currentSession.SelectedCustomer = _selectedCustomer;
            _currentSession.SelectedDiscount = _selectedDiscount;
        }

        private void LoadSession(SaleSession session)
        {
            _currentSession = session;

            // Desuscribir de items anteriores
            foreach (var item in _cartItems)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
            }

            // Cargar colecciones de la sesión
            _cartItems = session.CartItems;
            _paymentItems = session.PaymentItems;
            _selectedCustomer = session.SelectedCustomer;
            _selectedDiscount = session.SelectedDiscount;

            // Suscribir a items
            foreach (var item in _cartItems)
            {
                item.PropertyChanged += CartItem_PropertyChanged;
            }

            // Actualizar bindings de UI
            CartListView.ItemsSource = _cartItems;
            PaymentsListView.ItemsSource = _paymentItems;
            SetSelectedCustomer(_selectedCustomer ?? _customers.FirstOrDefault(c => c.Id == 1), updateTabName: false);

            if (_selectedDiscount != null)
            {
                SelectedDiscountText.Text = $"{_selectedDiscount.Percentage}% {_selectedDiscount.Name}";
                SelectedDiscountPanel.Visibility = Visibility.Visible;
            }
            else
            {
                SelectedDiscountPanel.Visibility = Visibility.Collapsed;
            }

            PaymentTypeComboBox.SelectedIndex = -1;
            PaymentAmountTextBox.Text = string.Empty;
            UpdateTotals();
            BuildSaleTabButtons();
        }

        private void CreateNewSaleSession()
        {
            // Guardar sesión actual si existe
            if (_currentSession != null)
            {
                SaveCurrentSessionState();
            }

            _saleCounter++;
            var session = new SaleSession
            {
                DisplayName = $"Venta {_saleCounter}",
                SelectedCustomer = _customers.FirstOrDefault(c => c.Id == 1)
            };
            _saleSessions.Add(session);
            LoadSession(session);
        }

        private async void RemoveSaleSession(SaleSession session)
        {
            // No permitir cerrar si es la única
            if (_saleSessions.Count <= 1)
            {
                return;
            }

            // Confirmación si tiene items
            if (session.CartItems.Count > 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Cerrar venta",
                    Content = $"La venta \"{session.DisplayName}\" tiene {session.CartItems.Count} producto(s). ¿Desea cerrarla?",
                    PrimaryButtonText = "Cerrar",
                    CloseButtonText = "Cancelar",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };
                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;
            }

            // Eliminar de DB si existe
            if (session.DbId.HasValue)
            {
                await DeletePendingSaleFromDb(session.DbId.Value);
            }

            // Desuscribir items de la sesión a eliminar
            foreach (var item in session.CartItems)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
            }

            _saleSessions.Remove(session);

            // Si era la sesión activa, cambiar a otra
            if (_currentSession == session)
            {
                LoadSession(_saleSessions[0]);
            }
            else
            {
                BuildSaleTabButtons();
            }
        }

        private async void RemoveCurrentSessionAfterInvoice()
        {
            // Eliminar de DB si existe
            if (_currentSession.DbId.HasValue)
            {
                await DeletePendingSaleFromDb(_currentSession.DbId.Value);
            }

            // Desuscribir items
            foreach (var item in _currentSession.CartItems)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
            }

            _saleSessions.Remove(_currentSession);

            if (_saleSessions.Count == 0)
            {
                CreateNewSaleSession();
            }
            else
            {
                LoadSession(_saleSessions[0]);
            }
        }

        private async void NewSaleTab_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentSessionState();
            await SaveCurrentSessionToDb();
            CreateNewSaleSession();
        }

        private void BuildSaleTabButtons()
        {
            SaleTabsPanel.Children.Clear();

            foreach (var session in _saleSessions)
            {
                var isActive = session == _currentSession;

                var nameText = new TextBlock
                {
                    Text = session.DisplayName,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = isActive ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal
                };

                var tabContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
                tabContent.Children.Add(nameText);

                // Mostrar cantidad de items si tiene
                if (session.CartItems.Count > 0)
                {
                    tabContent.Children.Add(new TextBlock
                    {
                        Text = $"({session.CartItems.Count})",
                        FontSize = 11,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
                    });
                }

                // Botón X para cerrar (solo si hay más de una sesión)
                if (_saleSessions.Count > 1)
                {
                    var closeButton = new Button
                    {
                        Content = "\uE711",
                        FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"),
                        Width = 20,
                        Height = 20,
                        Padding = new Thickness(0),
                        Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
                        Tag = session,
                        FontSize = 10,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    closeButton.Click += (s, args) =>
                    {
                        if (closeButton.Tag is SaleSession sess)
                        {
                            RemoveSaleSession(sess);
                        }
                    };
                    tabContent.Children.Add(closeButton);
                }

                var tabButton = new Button
                {
                    Content = tabContent,
                    Tag = session,
                    Padding = new Thickness(12, 6, 12, 6),
                    Style = isActive
                        ? (Style)Application.Current.Resources["AccentButtonStyle"]
                        : (Style)Application.Current.Resources["DefaultButtonStyle"]
                };
                tabButton.Click += async (s, args) =>
                {
                    if (tabButton.Tag is SaleSession sess && sess != _currentSession)
                    {
                        SaveCurrentSessionState();
                        await SaveCurrentSessionToDb();
                        LoadSession(sess);
                    }
                };
                SaleTabsPanel.Children.Add(tabButton);
            }

            // Botón "+" para nueva venta
            var addButton = new Button
            {
                Content = "+",
                Width = 36,
                Height = 36,
                Padding = new Thickness(0),
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold
            };
            addButton.Click += NewSaleTab_Click;
            SaleTabsPanel.Children.Add(addButton);
        }

        private async Task SaveCurrentSessionToDb()
        {
            try
            {
                var currentUser = App.CurrentUser;
                if (currentUser == null || _currentSession == null) return;

                // Solo guardar si tiene items
                if (_currentSession.CartItems.Count == 0)
                {
                    // Si no tiene items pero existe en DB, eliminar
                    if (_currentSession.DbId.HasValue)
                    {
                        await DeletePendingSaleFromDb(_currentSession.DbId.Value);
                        _currentSession.DbId = null;
                    }
                    return;
                }

                var data = new PendingSaleData
                {
                    CustomerId = _currentSession.SelectedCustomer?.Id,
                    DiscountId = _currentSession.SelectedDiscount?.Id
                };

                foreach (var item in _currentSession.CartItems)
                {
                    data.Items.Add(new PendingSaleItemData
                    {
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }

                foreach (var payment in _currentSession.PaymentItems)
                {
                    data.Payments.Add(new PendingSalePaymentData
                    {
                        PaymentTypeId = payment.PaymentType.Id,
                        Amount = payment.Amount
                    });
                }

                var json = JsonSerializer.Serialize(data);

                if (_currentSession.DbId.HasValue)
                {
                    var existing = await _dbContext.PendingSales.FindAsync(_currentSession.DbId.Value);
                    if (existing != null)
                    {
                        existing.DisplayName = _currentSession.DisplayName;
                        existing.JsonData = json;
                        _dbContext.PendingSales.Update(existing);
                    }
                }
                else
                {
                    var entity = new PendingSale
                    {
                        DisplayName = _currentSession.DisplayName,
                        JsonData = json,
                        UserId = currentUser.Id,
                        CreatedAt = DateTime.Now
                    };
                    _dbContext.PendingSales.Add(entity);
                    await _dbContext.SaveChangesAsync();
                    _currentSession.DbId = entity.Id;
                    return;
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving pending sale: {ex.Message}");
            }
        }

        private async Task LoadPendingSalesFromDb()
        {
            try
            {
                var currentUser = App.CurrentUser;
                if (currentUser == null) return;

                var pendingSales = await _dbContext.PendingSales
                    .Where(p => p.UserId == currentUser.Id)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();

                foreach (var pending in pendingSales)
                {
                    var data = JsonSerializer.Deserialize<PendingSaleData>(pending.JsonData);
                    if (data == null) continue;

                    _saleCounter++;
                    var session = new SaleSession
                    {
                        DbId = pending.Id,
                        DisplayName = pending.DisplayName
                    };

                    // Restaurar items
                    foreach (var itemData in data.Items)
                    {
                        var product = _allProducts.FirstOrDefault(p => p.Id == itemData.ProductId);
                        if (product == null) continue;

                        var cartItem = new CartItem
                        {
                            Product = product,
                            Quantity = itemData.Quantity,
                            UnitPrice = itemData.UnitPrice
                        };
                        session.CartItems.Add(cartItem);
                    }

                    // Restaurar pagos
                    foreach (var paymentData in data.Payments)
                    {
                        var paymentType = _paymentTypes.FirstOrDefault(pt => pt.Id == paymentData.PaymentTypeId);
                        if (paymentType == null) continue;

                        session.PaymentItems.Add(new PaymentItem
                        {
                            PaymentType = paymentType,
                            Amount = paymentData.Amount
                        });
                    }

                    // Restaurar cliente y descuento
                    if (data.CustomerId.HasValue)
                        session.SelectedCustomer = _customers.FirstOrDefault(c => c.Id == data.CustomerId.Value);
                    if (data.DiscountId.HasValue)
                        session.SelectedDiscount = _discounts.FirstOrDefault(d => d.Id == data.DiscountId.Value);

                    _saleSessions.Add(session);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading pending sales: {ex.Message}");
            }
        }

        private async Task DeletePendingSaleFromDb(int id)
        {
            try
            {
                var entity = await _dbContext.PendingSales.FindAsync(id);
                if (entity != null)
                {
                    _dbContext.PendingSales.Remove(entity);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting pending sale: {ex.Message}");
            }
        }

        #endregion

        #region Invoice Actions

        private async void Facturar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar turno abierto (admin no requiere turno)
                if (App.CurrentUser?.RoleId != (int)EDA.DOMAIN.Enums.RoleEnum.Admin
                    && (App.CurrentShift == null || !App.CurrentShift.IsOpen))
                {
                    await ShowError("Debe tener un turno abierto para facturar.");
                    return;
                }

                // Validar carrito no vacío
                if (_cartItems.Count == 0)
                {
                    await ShowError("Agregue al menos un producto al carrito.");
                    return;
                }

                // Validar CAI activo
                if (_activeCai == null)
                {
                    await ShowError("No hay un CAI activo. Configure un CAI antes de facturar.");
                    return;
                }

                // Validar cliente (usar Consumidor Final si no hay seleccionado)
                var selectedCustomer = _selectedCustomer;
                if (selectedCustomer == null)
                {
                    selectedCustomer = _customers.FirstOrDefault(c => c.Id == 1);
                    if (selectedCustomer == null)
                    {
                        await ShowError("Debe seleccionar un cliente.");
                        return;
                    }
                }

                // Validar pagos
                if (_paymentItems.Count == 0)
                {
                    await ShowError("Debe agregar al menos un método de pago.");
                    return;
                }

                // Validar usuario
                var currentUser = App.CurrentUser;
                if (currentUser == null)
                {
                    await ShowError("No hay un usuario identificado.");
                    return;
                }

                // Calcular totales (precio ya incluye impuesto)
                decimal totalBruto = _cartItems.Sum(c => c.Subtotal);
                decimal discountPercentage = _selectedDiscount?.Percentage ?? 0m;
                decimal discountAmount = totalBruto * discountPercentage / 100m;
                decimal taxAmount = 0m;

                foreach (var item in _cartItems)
                {
                    if (item.Product.Tax != null)
                    {
                        var itemTotal = item.Subtotal;
                        var itemAfterDiscount = itemTotal - (itemTotal * discountPercentage / 100m);
                        var taxRate = item.Product.Tax.Percentage / 100m;
                        taxAmount += itemAfterDiscount * (taxRate / (1m + taxRate));
                    }
                }

                decimal subtotal = totalBruto - discountAmount - taxAmount;
                decimal total = totalBruto - discountAmount;
                decimal totalPaid = _paymentItems.Sum(p => p.Amount);

                // Calcular efectivo y cambio
                double? cashReceived = null;
                double? changeGiven = null;

                if (totalPaid > total)
                {
                    cashReceived = (double)totalPaid;
                    changeGiven = (double)(totalPaid - total);
                }

                // Construir lista de items
                var items = _cartItems.Select(c => new CreateInvoiceItemDto
                {
                    ProductId = c.Product.Id,
                    Description = c.Product.Name,
                    Quantity = c.Quantity,
                    TaxId = c.Product.TaxId,
                    TaxPercentage = c.Product.Tax?.Percentage ?? 0m,
                    UnitPrice = c.UnitPrice
                }).ToList();

                // Construir lista de pagos
                var payments = _paymentItems.Select(p => new CreateInvoicePaymentDto
                {
                    PaymentTypeId = p.PaymentType.Id,
                    Amount = p.Amount
                }).ToList();

                // Crear comando
                var command = new CreateInvoiceCommand
                {
                    Date = DateTime.Now,
                    CustomerId = selectedCustomer.Id,
                    CaiId = _activeCai.Id,
                    UserId = currentUser.Id,
                    DiscountId = _selectedDiscount?.Id,
                    DiscountPercentage = discountPercentage,
                    Subtotal = subtotal,
                    TotalDiscounts = (double)discountAmount,
                    TotalTaxes = (double)taxAmount,
                    Total = total,
                    CashReceived = cashReceived,
                    ChangeGiven = changeGiven,
                    Items = items,
                    Payments = payments
                };

                // Enviar comando via MediatR
                var result = await _mediator.Send(command);

                if (result.Succeeded && result.Data != null)
                {
                    var invoice = result.Data;

                    // Generar e imprimir PDF
                    await GenerateAndPrintInvoicePdf(invoice, selectedCustomer, _activeCai);

                    await ShowSuccess($"Factura {invoice.InvoiceNumber} creada exitosamente.\n\nTotal: L. {invoice.Total:N2}");

                    // Eliminar sesión facturada y cambiar a otra
                    RemoveCurrentSessionAfterInvoice();

                    // Recargar CAI para obtener correlativo actualizado
                    await LoadActiveCai();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al crear la factura.");
                }
            }
            catch (EDA.APPLICATION.Exceptions.ValidationException vex)
            {
                await ShowError(string.Join("\n", vex.Errors));
            }
            catch (Exception ex)
            {
                await ShowError($"Error inesperado: {ex.Message}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Desuscribir de todos los items
            foreach (var item in _cartItems)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
            }

            _cartItems.Clear();
            _paymentItems.Clear();
            _selectedDiscount = null;
            SelectedDiscountPanel.Visibility = Visibility.Collapsed;

            // Resetear a cliente por defecto
            var defaultCustomer = _customers.FirstOrDefault(c => c.Id == 1);
            SetSelectedCustomer(defaultCustomer);

            PaymentTypeComboBox.SelectedIndex = -1;
            PaymentAmountTextBox.Text = string.Empty;
            UpdateTotals();
        }

        private async Task GenerateAndPrintInvoicePdf(Invoice invoice, Customer customer, Cai cai)
        {
            try
            {
                // Obtener datos de la empresa
                var company = await _dbContext.Companies.FirstOrDefaultAsync();
                if (company == null)
                {
                    // Si no hay empresa configurada, usar valores por defecto
                    company = new Company
                    {
                        Name = "Mi Empresa",
                        Owner = "Propietario"
                    };
                }

                // Cargar los productos vendidos con información del producto
                var soldProducts = await _dbContext.SoldProducts
                    .Where(sp => sp.InvoiceId == invoice.Id)
                    .ToListAsync();

                // Cargar los pagos con información del tipo de pago
                var invoicePayments = await _dbContext.InvoicePayments
                    .Include(ip => ip.PaymentType)
                    .Where(ip => ip.InvoiceId == invoice.Id)
                    .ToListAsync();

                // Construir datos para el PDF
                var pdfData = new InvoicePdfData
                {
                    // Datos de la empresa
                    CompanyName = company.Name,
                    CompanyOwner = company.Owner,
                    CompanyRtn = company.RTN,
                    CompanyAddress1 = company.Address1,
                    CompanyAddress2 = company.Address2,
                    CompanyPhone = company.PhoneNumber1,
                    CompanyEmail = company.Email,
                    CompanyLogo = company.Logo,

                    // Datos de la factura
                    InvoiceNumber = invoice.InvoiceNumber,
                    Date = invoice.Date,
                    Subtotal = invoice.Subtotal,
                    TotalDiscounts = invoice.TotalDiscounts,
                    TotalTaxes = invoice.TotalTaxes,
                    Total = invoice.Total,
                    TaxedAt15Percent = invoice.TaxedAt15Percent,
                    TaxesAt15Percent = invoice.TaxesAt15Percent,
                    TaxedAt18Percent = invoice.TaxedAt18Percent,
                    TaxesAt18Percent = invoice.TaxesAt18Percent,
                    Exempt = invoice.Exempt,
                    CashReceived = invoice.CashReceived,
                    ChangeGiven = invoice.ChangeGiven,

                    // Datos del cliente
                    CustomerName = customer.Name,
                    CustomerRtn = customer.RTN,

                    // Datos del CAI
                    CaiNumber = cai.Code,
                    CaiFromDate = cai.FromDate,
                    CaiToDate = cai.ToDate,
                    InitialCorrelative = $"{cai.Prefix}{cai.InitialCorrelative:D8}",
                    FinalCorrelative = $"{cai.Prefix}{cai.FinalCorrelative:D8}",

                    // Items
                    Items = soldProducts.Select(sp => new InvoicePdfItem
                    {
                        Description = sp.Description ?? "Producto",
                        Quantity = sp.Quantity,
                        UnitPrice = sp.UnitPrice,
                        TaxPercentage = _taxes.FirstOrDefault(t => t.Id == sp.TaxId)?.Percentage ?? 0m,
                        TotalLine = sp.TotalLine
                    }).ToList(),

                    // Pagos
                    Payments = invoicePayments.Select(ip => new InvoicePdfPayment
                    {
                        PaymentTypeName = ip.PaymentType?.Name ?? "Pago",
                        Amount = ip.Amount
                    }).ToList()
                };

                // Generar PDF
                var pdfService = App.Services.GetRequiredService<IInvoicePdfService>();
                var pdfBytes = pdfService.GenerateInvoicePdf(pdfData);

                // Guardar PDF en archivo temporal
                var tempPath = Path.Combine(Path.GetTempPath(), $"Factura_{invoice.InvoiceNumber.Replace("-", "_")}.pdf");
                await File.WriteAllBytesAsync(tempPath, pdfBytes);

                // Abrir PDF con la aplicación predeterminada
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                // No fallar si hay error al generar PDF, solo mostrar advertencia
                Debug.WriteLine($"Error al generar PDF: {ex.Message}");
            }
        }

        #endregion

        #region Dialogs

        private async Task ShowError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uEA39",
                            FontSize = 48,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                },
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowSuccess(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Exito",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uE73E",
                            FontSize = 48,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                },
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        #endregion
    }
}
