using EDA.APPLICATION.Features.CaiFeature.Queries;
using EDA.APPLICATION.Features.CompanyFeature.Queries;
using EDA.APPLICATION.Features.CustomerFeature.Commands.CreateCustomerCommand;
using EDA.APPLICATION.Features.CustomerFeature.Queries;
using EDA.APPLICATION.Features.DiscountFeature.Queries;
using EDA.APPLICATION.Features.InvoiceFeature.Commands.CreateInvoiceCommand;
using EDA.APPLICATION.Features.PaymentTypeFeature.Queries;
using EDA.APPLICATION.Features.ProductFeature.Queries;
using EDA.APPLICATION.Features.TaxFeature.Queries;
using EDA.APPLICATION.Interfaces;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class InvoiceCreatePage : Page
    {
        private readonly IMediator _mediator;

        private List<Product> _allProducts = new();
        private List<Customer> _customers = new();
        private List<Discount> _discounts = new();
        private List<PaymentType> _paymentTypes = new();
        private List<Tax> _taxes = new();
        private Cai? _activeCai;
        private Discount? _selectedDiscount;
        private Customer? _selectedCustomer;

        private ObservableCollection<CartItem> _cartItems = new();
        private ObservableCollection<PaymentItem> _paymentItems = new();

        private bool _isCredit = false;
        private int _creditDays = 0;

        public InvoiceCreatePage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();

            CartListView.ItemsSource = _cartItems;
            PaymentsListView.ItemsSource = _paymentItems;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInitialData();
        }

        #region Data Loading

        private async Task LoadInitialData()
        {
            LoadingRing.IsActive = true;

            try
            {
                var productsResult = await _mediator.Send(new GetAllProductsQuery { GetAll = true });
                if (productsResult.Succeeded && productsResult.Data != null)
                    _allProducts = productsResult.Data.Items.ToList();

                var customersResult = await _mediator.Send(new GetAllCustomersQuery { GetAll = true });
                if (customersResult.Succeeded && customersResult.Data != null)
                {
                    _customers = customersResult.Data.Items.ToList();
                    var defaultCustomer = _customers.FirstOrDefault(c => c.Id == 1);
                    SetSelectedCustomer(defaultCustomer);
                }

                var discountsResult = await _mediator.Send(new GetAllDiscountsQuery { GetAll = true });
                if (discountsResult.Succeeded && discountsResult.Data != null)
                {
                    _discounts = discountsResult.Data.Items.ToList();
                    DiscountComboBox.ItemsSource = _discounts;
                }

                var paymentTypesResult = await _mediator.Send(new GetAllPaymentTypesQuery { GetAll = true });
                if (paymentTypesResult.Succeeded && paymentTypesResult.Data != null)
                {
                    _paymentTypes = paymentTypesResult.Data.Items.ToList();
                    PaymentTypeComboBox.ItemsSource = _paymentTypes;
                }

                var taxesResult = await _mediator.Send(new GetAllTaxesQuery { GetAll = true });
                if (taxesResult.Succeeded && taxesResult.Data != null)
                    _taxes = taxesResult.Data.Items.ToList();

                await LoadActiveCai();
            }
            catch (Exception ex)
            {
                await ShowError($"Error al cargar datos: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
        }

        private async Task LoadActiveCai()
        {
            try
            {
                var result = await _mediator.Send(new GetActiveCaiQuery());
                if (result.Succeeded && result.Data != null)
                {
                    _activeCai = result.Data;
                    CaiInfoText.Text = $"CAI: {_activeCai.Name} | Correlativo: {_activeCai.CurrentCorrelative}";
                }
                else
                {
                    _activeCai = null;
                    CaiInfoText.Text = "Sin CAI activo";
                }
            }
            catch
            {
                CaiInfoText.Text = "Error al cargar CAI";
            }
        }

        #endregion

        #region Product Selection Modal

        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowProductSelectionDialog();
        }

        private async Task ShowProductSelectionDialog()
        {
            var searchBox = new TextBox
            {
                PlaceholderText = "Buscar por nombre o codigo...",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var productsList = new ListView
            {
                SelectionMode = ListViewSelectionMode.Single,
                Height = 350,
                ItemsSource = _allProducts
            };

            productsList.ItemTemplate = CreateProductItemTemplate();

            searchBox.TextChanged += (s, args) =>
            {
                var term = searchBox.Text?.Trim().ToLower() ?? "";
                productsList.ItemsSource = string.IsNullOrEmpty(term)
                    ? _allProducts
                    : _allProducts.Where(p =>
                        p.Name.ToLower().Contains(term) ||
                        (p.Barcode != null && p.Barcode.ToLower().Contains(term)))
                      .ToList();
            };

            var content = new StackPanel { Width = 500, Children = { searchBox, productsList } };

            var dialog = new ContentDialog
            {
                Title = "Seleccionar Producto",
                Content = content,
                PrimaryButtonText = "Agregar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            productsList.DoubleTapped += (s, args) =>
            {
                if (productsList.SelectedItem is Product product)
                {
                    AddProductToCart(product);
                    dialog.Hide();
                }
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && productsList.SelectedItem is Product selectedProduct)
            {
                AddProductToCart(selectedProduct);
            }
        }

        private DataTemplate CreateProductItemTemplate()
        {
            return (DataTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(@"
                <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <Grid Padding='8,4'>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width='*'/>
                            <ColumnDefinition Width='80'/>
                            <ColumnDefinition Width='80'/>
                        </Grid.ColumnDefinitions>
                        <TextBlock Grid.Column='0' Text='{Binding Name}' VerticalAlignment='Center' TextTrimming='CharacterEllipsis'/>
                        <TextBlock Grid.Column='1' Text='{Binding Barcode}' VerticalAlignment='Center' Foreground='Gray'/>
                        <TextBlock Grid.Column='2' VerticalAlignment='Center' HorizontalAlignment='Right'>
                            <Run Text='L '/>
                            <Run Text='{Binding Price}'/>
                        </TextBlock>
                    </Grid>
                </DataTemplate>");
        }

        private void AddProductToCart(Product product)
        {
            var existing = _cartItems.FirstOrDefault(c => c.Product.Id == product.Id);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                var cartItem = new CartItem
                {
                    Product = product,
                    Quantity = 1,
                    UnitPrice = product.Price
                };
                cartItem.PropertyChanged += CartItem_PropertyChanged;
                _cartItems.Add(cartItem);
            }
            UpdateTotals();
        }

        #endregion

        #region Price Editing

        private void PriceNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is NumberBox numberBox && numberBox.Tag is CartItem item)
            {
                if (!double.IsNaN(numberBox.Value) && numberBox.Value >= 0)
                {
                    item.UnitPrice = (decimal)numberBox.Value;
                }
                else
                {
                    numberBox.Value = (double)item.UnitPrice;
                }
                UpdateTotals();
            }
        }

        #endregion

        #region Cart Management

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem item)
            {
                item.Quantity++;
                UpdateTotals();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem item)
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
            if (sender is Button btn && btn.Tag is CartItem item)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
                _cartItems.Remove(item);
                UpdateTotals();
            }
        }

        private void CartItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Subtotal))
            {
                UpdateTotals();
            }
        }

        #endregion

        #region Customer

        private async void CustomerSelectButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCustomerSearchDialog();
        }

        private async void NewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCreateCustomerDialog();
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

            resultsListView.ItemsSource = _customers.Take(50).ToList();

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

        private async Task ShowCreateCustomerDialog()
        {
            var nameBox = new TextBox { PlaceholderText = "Nombre del cliente", Margin = new Thickness(0, 0, 0, 8) };
            var rtnBox = new TextBox { PlaceholderText = "RTN (opcional)", Margin = new Thickness(0, 0, 0, 8) };

            var content = new StackPanel
            {
                Width = 350,
                Children = { nameBox, rtnBox }
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

            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
            {
                var command = new CreateCustomerCommand
                {
                    Name = nameBox.Text.Trim(),
                    RTN = string.IsNullOrWhiteSpace(rtnBox.Text) ? null : rtnBox.Text.Trim()
                };

                var createResult = await _mediator.Send(command);
                if (createResult.Succeeded && createResult.Data != null)
                {
                    _customers.Add(createResult.Data);
                    SetSelectedCustomer(createResult.Data);
                }
            }
        }

        private void SetSelectedCustomer(Customer? customer)
        {
            _selectedCustomer = customer;
            CustomerSelectButton.Content = customer?.Name ?? "Seleccionar cliente...";
        }

        #endregion

        #region Discount

        private void DiscountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DiscountComboBox.SelectedItem is Discount discount)
            {
                _selectedDiscount = discount;
                RemoveDiscountButton.Visibility = Visibility.Visible;
            }
            else
            {
                _selectedDiscount = null;
                RemoveDiscountButton.Visibility = Visibility.Collapsed;
            }
            UpdateTotals();
        }

        private void RemoveDiscount_Click(object sender, RoutedEventArgs e)
        {
            _selectedDiscount = null;
            DiscountComboBox.SelectedIndex = -1;
            RemoveDiscountButton.Visibility = Visibility.Collapsed;
            UpdateTotals();
        }

        #endregion

        #region Payments

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

            PaymentAmountTextBox.Text = string.Empty;
            UpdateTotals();
        }

        private void RemovePayment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PaymentItem payment)
            {
                _paymentItems.Remove(payment);
                UpdateTotals();
            }
        }

        #endregion

        #region Totals

        private void UpdateTotals()
        {
            decimal totalBruto = _cartItems.Sum(c => c.Subtotal);

            decimal discountPercentage = _selectedDiscount?.Percentage ?? 0m;
            decimal discountAmount = totalBruto * (discountPercentage / 100m);

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

            decimal subtotal = totalBruto - discountAmount - taxAmount;
            decimal total = totalBruto - discountAmount;

            decimal totalPaid = _paymentItems.Sum(p => p.Amount);
            decimal pending = total - totalPaid;

            SubtotalText.Text = $"L. {subtotal:N2}";
            DiscountAmountText.Text = $"- L. {discountAmount:N2}";
            TaxAmountText.Text = $"L. {taxAmount:N2}";
            TotalText.Text = $"L. {total:N2}";
            TotalPaidText.Text = $"L. {totalPaid:N2}";

            if (pending > 0)
            {
                PendingLabelText.Text = "Pendiente";
                PendingAmountText.Text = $"L. {pending:N2}";
                PendingAmountText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            }
            else if (pending < 0)
            {
                decimal change = Math.Abs(pending);
                PendingLabelText.Text = "Cambio";
                PendingAmountText.Text = $"L. {change:N2}";
                PendingAmountText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                PendingLabelText.Text = "Pendiente";
                PendingAmountText.Text = "L. 0.00";
                PendingAmountText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray);
            }

            CreateInvoiceButton.IsEnabled = _cartItems.Count > 0 && total > 0 && (_isCredit || totalPaid >= total);
        }

        #endregion

        #region Credit

        private void CreditToggle_Toggled(object sender, RoutedEventArgs e)
        {
            _isCredit = CreditToggle.IsOn;
            CreditOptionsPanel.Visibility = _isCredit ? Visibility.Visible : Visibility.Collapsed;

            if (!_isCredit)
            {
                _creditDays = 0;
                CreditDaysNumberBox.Value = double.NaN;
                DueDatePreview.Text = string.Empty;
            }

            UpdateTotals();
        }

        private void CreditDays_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tagValue && int.TryParse(tagValue, out int days))
            {
                _creditDays = days;
                CreditDaysNumberBox.Value = days;
                UpdateDueDatePreview();
            }
        }

        private void CreditDaysNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (double.IsNaN(args.NewValue))
            {
                _creditDays = 0;
                DueDatePreview.Text = string.Empty;
            }
            else
            {
                _creditDays = (int)args.NewValue;
                UpdateDueDatePreview();
            }
        }

        private void UpdateDueDatePreview()
        {
            if (_creditDays > 0)
            {
                var dueDate = DateTime.Now.AddDays(_creditDays);
                DueDatePreview.Text = $"Fecha de vencimiento: {dueDate:dd/MM/yyyy}";
            }
            else
            {
                DueDatePreview.Text = string.Empty;
            }
        }

        #endregion

        #region Invoice Creation

        private async void CreateInvoice_Click(object sender, RoutedEventArgs e)
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

                if (_cartItems.Count == 0)
                {
                    await ShowError("Agregue al menos un producto.");
                    return;
                }

                if (_activeCai == null)
                {
                    await ShowError("No hay un CAI activo. Configure un CAI antes de facturar.");
                    return;
                }

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

                // Validar credito
                if (_isCredit)
                {
                    if (_creditDays <= 0)
                    {
                        await ShowError("Debe especificar los días de crédito.");
                        return;
                    }

                    if (selectedCustomer.Id == 1)
                    {
                        await ShowError("Las facturas al crédito requieren un cliente identificado (no Consumidor Final).");
                        return;
                    }
                }
                else
                {
                    if (_paymentItems.Count == 0)
                    {
                        await ShowError("Debe agregar al menos un método de pago.");
                        return;
                    }
                }

                var currentUser = App.CurrentUser;
                if (currentUser == null)
                {
                    await ShowError("No hay un usuario identificado.");
                    return;
                }

                // Calcular totales
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

                double? cashReceived = null;
                double? changeGiven = null;

                if (totalPaid > total)
                {
                    cashReceived = (double)totalPaid;
                    changeGiven = (double)(totalPaid - total);
                }

                var items = _cartItems.Select(c => new CreateInvoiceItemDto
                {
                    ProductId = c.Product.Id,
                    Description = c.Product.Name,
                    Quantity = c.Quantity,
                    TaxId = c.Product.TaxId,
                    TaxPercentage = c.Product.Tax?.Percentage ?? 0m,
                    UnitPrice = c.UnitPrice
                }).ToList();

                var payments = _paymentItems.Select(p => new CreateInvoicePaymentDto
                {
                    PaymentTypeId = p.PaymentType.Id,
                    Amount = p.Amount
                }).ToList();

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
                    Payments = payments,
                    IsCredit = _isCredit,
                    CreditDays = _isCredit ? _creditDays : null
                };

                var result = await _mediator.Send(command);

                if (result.Succeeded && result.Data != null)
                {
                    var invoice = result.Data;

                    await GenerateAndPrintInvoicePdf(invoice, selectedCustomer, _activeCai);

                    var successMessage = $"Factura {invoice.InvoiceNumber} creada exitosamente.\n\nTotal: L. {invoice.Total:N2}";
                    if (_isCredit)
                    {
                        successMessage += $"\n\nCrédito: {_creditDays} días";
                        if (invoice.DueDate.HasValue)
                            successMessage += $"\nVencimiento: {invoice.DueDate.Value:dd/MM/yyyy}";
                        if (invoice.OutstandingAmount > 0)
                            successMessage += $"\nSaldo pendiente: L. {invoice.OutstandingAmount:N2}";
                    }
                    await ShowSuccess(successMessage);

                    // Navigate back to invoices list
                    var mainWindow = App.MainWindow as MainWindow;
                    mainWindow?.NavigateToInvoices();
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

        #endregion

        #region PDF Generation

        private async Task GenerateAndPrintInvoicePdf(Invoice invoice, Customer customer, Cai cai)
        {
            try
            {
                var companyResult = await _mediator.Send(new GetCompanyQuery());
                var company = companyResult.Data ?? new Company
                {
                    Name = "Mi Empresa",
                    Owner = "Propietario"
                };

                var soldProducts = _cartItems.Select(c => new SoldProduct
                {
                    Description = c.Product.Name,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice,
                    TaxId = c.Product.TaxId,
                    TotalLine = c.Subtotal
                }).ToList();

                var invoicePayments = _paymentItems.Select(p => new InvoicePayment
                {
                    PaymentType = p.PaymentType,
                    Amount = p.Amount
                }).ToList();

                var pdfData = new EDA.APPLICATION.DTOs.InvoicePdfData
                {
                    CompanyName = company.Name,
                    CompanyOwner = company.Owner,
                    CompanyRtn = company.RTN,
                    CompanyAddress1 = company.Address1,
                    CompanyAddress2 = company.Address2,
                    CompanyPhone = company.PhoneNumber1,
                    CompanyEmail = company.Email,
                    CompanyLogo = company.Logo,

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

                    CustomerName = customer.Name,
                    CustomerRtn = customer.RTN,

                    CaiNumber = cai.Code,
                    CaiFromDate = cai.FromDate,
                    CaiToDate = cai.ToDate,
                    InitialCorrelative = $"{cai.Prefix}{cai.InitialCorrelative:D8}",
                    FinalCorrelative = $"{cai.Prefix}{cai.FinalCorrelative:D8}",

                    Items = soldProducts.Select(sp => new EDA.APPLICATION.DTOs.InvoicePdfItem
                    {
                        Description = sp.Description ?? "Producto",
                        Quantity = sp.Quantity,
                        UnitPrice = sp.UnitPrice,
                        TaxPercentage = _taxes.FirstOrDefault(t => t.Id == sp.TaxId)?.Percentage ?? 0m,
                        TotalLine = sp.TotalLine
                    }).ToList(),

                    Payments = invoicePayments.Select(ip => new EDA.APPLICATION.DTOs.InvoicePdfPayment
                    {
                        PaymentTypeName = ip.PaymentType?.Name ?? "Pago",
                        Amount = ip.Amount
                    }).ToList()
                };

                var pdfService = App.Services.GetRequiredService<IInvoicePdfService>();
                var pdfBytes = pdfService.GenerateInvoicePdf(pdfData);

                var tempPath = Path.Combine(Path.GetTempPath(), $"Factura_{invoice.InvoiceNumber.Replace("-", "_")}.pdf");
                await File.WriteAllBytesAsync(tempPath, pdfBytes);

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al generar PDF: {ex.Message}");
            }
        }

        #endregion

        #region Navigation

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.MainWindow as MainWindow;
            mainWindow?.NavigateToInvoices();
        }

        #endregion

        #region Clear

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _cartItems)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
            }

            _cartItems.Clear();
            _paymentItems.Clear();
            _selectedDiscount = null;
            DiscountComboBox.SelectedIndex = -1;
            RemoveDiscountButton.Visibility = Visibility.Collapsed;

            var defaultCustomer = _customers.FirstOrDefault(c => c.Id == 1);
            SetSelectedCustomer(defaultCustomer);

            _isCredit = false;
            _creditDays = 0;
            CreditToggle.IsOn = false;
            CreditOptionsPanel.Visibility = Visibility.Collapsed;
            CreditDaysNumberBox.Value = double.NaN;
            DueDatePreview.Text = string.Empty;

            PaymentTypeComboBox.SelectedIndex = -1;
            PaymentAmountTextBox.Text = string.Empty;
            UpdateTotals();
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
