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

namespace EDA_2._0.Views
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;

        public Product Product { get; set; } = null!;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        public decimal UnitPrice { get; set; }
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

        private ObservableCollection<CartItem> _cartItems = new();
        private ObservableCollection<PaymentItem> _paymentItems = new();

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
                    CustomerComboBox.ItemsSource = _customers;
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

                // Cargar familias y impuestos (para crear productos)
                var familiesResult = await _mediator.Send(new GetAllFamiliesQuery { GetAll = true });
                if (familiesResult.Succeeded && familiesResult.Data != null)
                {
                    _families = familiesResult.Data.Items.ToList();
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
                var searchTerm = ProductSearchTextBox.Text?.Trim().ToLower();
                if (string.IsNullOrEmpty(searchTerm))
                {
                    ProductsItemsControl.ItemsSource = _allProducts;
                }
                else
                {
                    var filtered = _allProducts.Where(p =>
                        p.Name.ToLower().Contains(searchTerm) ||
                        (p.Barcode != null && p.Barcode.ToLower().Contains(searchTerm))
                    ).ToList();
                    ProductsItemsControl.ItemsSource = filtered;
                }
            }
        }

        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                AddProductToCart(product);
            }
        }

        private void AddProductToCart(Product product)
        {
            var existingItem = _cartItems.FirstOrDefault(c => c.Product.Id == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                _cartItems.Add(new CartItem
                {
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price
                });
            }

            UpdateTotals();
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
                }
                else
                {
                    _cartItems.Remove(item);
                }
                UpdateTotals();
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CartItem item)
            {
                _cartItems.Remove(item);
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
            // Calcular subtotal
            decimal subtotal = _cartItems.Sum(c => c.Subtotal);

            // Calcular descuento
            decimal discountAmount = 0;
            if (_selectedDiscount != null)
            {
                discountAmount = subtotal * (_selectedDiscount.Percentage / 100m);
            }

            // Calcular impuesto (promedio de los impuestos de los productos)
            decimal taxAmount = 0;
            foreach (var item in _cartItems)
            {
                if (item.Product.Tax != null)
                {
                    var itemSubtotal = item.Subtotal;
                    var itemAfterDiscount = itemSubtotal - (itemSubtotal * (_selectedDiscount?.Percentage ?? 0) / 100m);
                    taxAmount += itemAfterDiscount * (item.Product.Tax.Percentage / 100m);
                }
            }

            // Calcular total
            decimal total = subtotal - discountAmount + taxAmount;

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
                        CustomerComboBox.ItemsSource = null;
                        CustomerComboBox.ItemsSource = _customers;
                        CustomerComboBox.SelectedItem = createResult.Data;
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

        #region Invoice Actions

        private async void Facturar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                var selectedCustomer = CustomerComboBox.SelectedItem as Customer;
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

                // Calcular totales
                decimal subtotal = _cartItems.Sum(c => c.Subtotal);
                decimal discountPercentage = _selectedDiscount?.Percentage ?? 0m;
                decimal discountAmount = subtotal * discountPercentage / 100m;
                decimal taxAmount = 0m;

                foreach (var item in _cartItems)
                {
                    if (item.Product.Tax != null)
                    {
                        var itemSubtotal = item.Subtotal;
                        var itemAfterDiscount = itemSubtotal - (itemSubtotal * discountPercentage / 100m);
                        taxAmount += itemAfterDiscount * (item.Product.Tax.Percentage / 100m);
                    }
                }

                decimal total = subtotal - discountAmount + taxAmount;
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

                    // Limpiar el carrito y resetear para siguiente factura
                    ClearButton_Click(sender, e);

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
            _cartItems.Clear();
            _paymentItems.Clear();
            _selectedDiscount = null;
            SelectedDiscountPanel.Visibility = Visibility.Collapsed;
            CustomerComboBox.SelectedIndex = -1;
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
