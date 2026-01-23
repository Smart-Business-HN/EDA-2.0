using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.InvoiceFeature.Commands.AddPaymentToInvoiceCommand;
using EDA.APPLICATION.Features.InvoiceFeature.Commands.VoidInvoiceCommand;
using EDA.APPLICATION.Features.InvoiceFeature.Queries.GetInvoiceByIdQuery;
using EDA.APPLICATION.Interfaces;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using EDA.INFRAESTRUCTURE;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class InvoiceDetailPage : Page
    {
        private readonly IMediator _mediator;
        private readonly DatabaseContext _dbContext;
        private int _invoiceId;
        private Invoice? _invoice;

        public InvoiceDetailPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
            _dbContext = App.Services.GetRequiredService<DatabaseContext>();
        }

        public void SetInvoiceId(int invoiceId)
        {
            _invoiceId = invoiceId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInvoice();
        }

        private async Task LoadInvoice()
        {
            SetLoading(true);

            try
            {
                var result = await _mediator.Send(new GetInvoiceByIdQuery { Id = _invoiceId });

                if (result.Succeeded && result.Data != null)
                {
                    _invoice = result.Data;
                    PopulateInvoiceData();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar la factura");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void PopulateInvoiceData()
        {
            if (_invoice == null) return;

            // Header
            InvoiceNumberText.Text = $"Factura #{_invoice.InvoiceNumber}";
            InvoiceDateText.Text = $"Fecha: {_invoice.Date:dd/MM/yyyy HH:mm:ss}";

            // Status Badge
            UpdateStatusBadge();

            // Customer Info
            CustomerNameText.Text = _invoice.Customer?.Name ?? "-";
            CustomerRtnText.Text = _invoice.Customer?.RTN ?? "-";

            // Invoice Info
            UserNameText.Text = _invoice.User != null
                ? $"{_invoice.User.Name} {_invoice.User.LastName}"
                : "-";
            CaiText.Text = _invoice.Cai?.Code ?? "-";
            DiscountText.Text = _invoice.Discount?.Name ?? "Sin descuento";

            // Products
            ProductsListView.ItemsSource = _invoice.SoldProducts;

            // Payments
            PaymentsListView.ItemsSource = _invoice.InvoicePayments;

            // Totals
            SubtotalText.Text = $"L {_invoice.Subtotal:N2}";
            TotalDiscountsText.Text = $"L -{_invoice.TotalDiscounts:N2}";
            ExemptText.Text = $"L {_invoice.Exempt:N2}";
            Taxed15Text.Text = $"L {_invoice.TaxedAt15Percent:N2}";
            Taxes15Text.Text = $"L {_invoice.TaxesAt15Percent:N2}";
            Taxed18Text.Text = $"L {_invoice.TaxedAt18Percent:N2}";
            Taxes18Text.Text = $"L {_invoice.TaxesAt18Percent:N2}";
            TotalText.Text = $"L {_invoice.Total:N2}";
            CashReceivedText.Text = $"L {_invoice.CashReceived:N2}";
            ChangeGivenText.Text = $"L {_invoice.ChangeGiven:N2}";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.MainWindow as MainWindow;
            mainWindow?.NavigateToInvoices();
        }

        private void UpdateStatusBadge()
        {
            if (_invoice == null) return;

            var statusId = (InvoiceStatusEnum)_invoice.StatusId;
            string statusName = _invoice.Status?.Name ?? statusId.ToString();

            StatusText.Text = statusName.ToUpper();

            switch (statusId)
            {
                case InvoiceStatusEnum.Creada:
                    StatusBadge.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 52, 152, 219)); // Azul
                    break;
                case InvoiceStatusEnum.Pagada:
                    StatusBadge.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 39, 174, 96)); // Verde
                    break;
                case InvoiceStatusEnum.Anulada:
                    StatusBadge.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 231, 76, 60)); // Rojo
                    break;
                default:
                    StatusBadge.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 127, 140, 141)); // Gris
                    break;
            }

            // Ocultar botón de anular si ya está anulada
            VoidInvoiceButton.Visibility = statusId == InvoiceStatusEnum.Anulada
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Mostrar botón de agregar pago solo si está en estado Creada
            AddPaymentButton.Visibility = statusId == InvoiceStatusEnum.Creada
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private async void VoidInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_invoice == null) return;

            // Confirmar anulación
            var confirmDialog = new ContentDialog
            {
                Title = "Anular Factura",
                Content = $"¿Está seguro que desea anular la factura #{_invoice.InvoiceNumber}?\n\nEsta acción no se puede deshacer.",
                PrimaryButtonText = "Anular",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            SetLoading(true);

            try
            {
                var voidResult = await _mediator.Send(new VoidInvoiceCommand { InvoiceId = _invoice.Id });

                if (voidResult.Succeeded)
                {
                    // Actualizar estado local
                    _invoice.StatusId = (int)InvoiceStatusEnum.Anulada;
                    UpdateStatusBadge();

                    var successDialog = new ContentDialog
                    {
                        Title = "Factura Anulada",
                        Content = "La factura ha sido anulada correctamente.",
                        CloseButtonText = "Aceptar",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    await ShowError(voidResult.Message ?? "Error al anular la factura");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_invoice == null) return;

            // Calcular monto pendiente
            var totalPaid = _invoice.InvoicePayments?.Sum(p => p.Amount) ?? 0;
            var pendingAmount = _invoice.Total - totalPaid;

            // Obtener tipos de pago
            var paymentTypes = await _dbContext.PaymentTypes.ToListAsync();

            // Crear controles para el diálogo
            var paymentTypeCombo = new ComboBox
            {
                ItemsSource = paymentTypes,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id",
                PlaceholderText = "Seleccione tipo de pago",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };
            if (paymentTypes.Count > 0)
                paymentTypeCombo.SelectedIndex = 0;

            var amountBox = new NumberBox
            {
                Header = "Monto",
                Value = (double)pendingAmount,
                Minimum = 0.01,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var pendingText = new TextBlock
            {
                Text = $"Pendiente: L {pendingAmount:N2}",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var contentPanel = new StackPanel
            {
                Children = { pendingText, paymentTypeCombo, amountBox }
            };

            var dialog = new ContentDialog
            {
                Title = "Agregar Pago",
                Content = contentPanel,
                PrimaryButtonText = "Agregar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            if (paymentTypeCombo.SelectedValue == null)
            {
                await ShowError("Debe seleccionar un tipo de pago.");
                return;
            }

            var paymentTypeId = (int)paymentTypeCombo.SelectedValue;
            var amount = (decimal)amountBox.Value;

            if (amount <= 0)
            {
                await ShowError("El monto debe ser mayor a cero.");
                return;
            }

            SetLoading(true);

            try
            {
                var addResult = await _mediator.Send(new AddPaymentToInvoiceCommand
                {
                    InvoiceId = _invoice.Id,
                    PaymentTypeId = paymentTypeId,
                    Amount = amount
                });

                if (addResult.Succeeded)
                {
                    // Recargar la factura para ver los cambios
                    await LoadInvoice();

                    var successDialog = new ContentDialog
                    {
                        Title = "Pago Agregado",
                        Content = "El pago ha sido registrado correctamente.",
                        CloseButtonText = "Aceptar",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    await ShowError(addResult.Message ?? "Error al agregar el pago");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void PrintTicketButton_Click(object sender, RoutedEventArgs e)
        {
            if (_invoice == null) return;
            await GenerateAndPrintPdf(isLetterFormat: false);
        }

        private async void PrintLetterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_invoice == null) return;
            await GenerateAndPrintPdf(isLetterFormat: true);
        }

        private async Task GenerateAndPrintPdf(bool isLetterFormat)
        {
            if (_invoice == null) return;

            SetLoading(true);

            try
            {
                // Obtener datos de la empresa
                var company = await _dbContext.Companies.FirstOrDefaultAsync();
                if (company == null)
                {
                    company = new Company
                    {
                        Name = "Mi Empresa",
                        Owner = "Propietario"
                    };
                }

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
                    InvoiceNumber = _invoice.InvoiceNumber,
                    Date = _invoice.Date,
                    Subtotal = _invoice.Subtotal,
                    TotalDiscounts = _invoice.TotalDiscounts,
                    TotalTaxes = _invoice.TotalTaxes,
                    Total = _invoice.Total,
                    TaxedAt15Percent = _invoice.TaxedAt15Percent,
                    TaxesAt15Percent = _invoice.TaxesAt15Percent,
                    TaxedAt18Percent = _invoice.TaxedAt18Percent,
                    TaxesAt18Percent = _invoice.TaxesAt18Percent,
                    Exempt = _invoice.Exempt,
                    CashReceived = _invoice.CashReceived,
                    ChangeGiven = _invoice.ChangeGiven,
                    IsVoided = _invoice.StatusId == (int)InvoiceStatusEnum.Anulada,

                    // Datos del cliente
                    CustomerName = _invoice.Customer?.Name ?? "Cliente",
                    CustomerRtn = _invoice.Customer?.RTN,

                    // Datos del CAI
                    CaiNumber = _invoice.Cai?.Code ?? "",
                    CaiFromDate = _invoice.Cai?.FromDate ?? DateTime.Now,
                    CaiToDate = _invoice.Cai?.ToDate ?? DateTime.Now,
                    InitialCorrelative = _invoice.Cai != null
                        ? $"{_invoice.Cai.Prefix}{_invoice.Cai.InitialCorrelative:D8}"
                        : "",
                    FinalCorrelative = _invoice.Cai != null
                        ? $"{_invoice.Cai.Prefix}{_invoice.Cai.FinalCorrelative:D8}"
                        : "",

                    // Items
                    Items = _invoice.SoldProducts?.Select(sp => new InvoicePdfItem
                    {
                        Description = sp.Description ?? "Producto",
                        Quantity = sp.Quantity,
                        UnitPrice = sp.UnitPrice,
                        TaxPercentage = sp.Tax?.Percentage ?? 0m,
                        TotalLine = sp.TotalLine
                    }).ToList() ?? new(),

                    // Pagos
                    Payments = _invoice.InvoicePayments?.Select(ip => new InvoicePdfPayment
                    {
                        PaymentTypeName = ip.PaymentType?.Name ?? "Pago",
                        Amount = ip.Amount
                    }).ToList() ?? new()
                };

                // Generar PDF
                var pdfService = App.Services.GetRequiredService<IInvoicePdfService>();
                byte[] pdfBytes;

                if (isLetterFormat)
                {
                    pdfBytes = pdfService.GenerateInvoiceLetterPdf(pdfData);
                }
                else
                {
                    pdfBytes = pdfService.GenerateInvoicePdf(pdfData);
                }

                // Guardar PDF en archivo temporal
                var formatSuffix = isLetterFormat ? "_Carta" : "_Ticket";
                var tempPath = Path.Combine(Path.GetTempPath(), $"Factura_{_invoice.InvoiceNumber.Replace("-", "_")}{formatSuffix}.pdf");
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
                await ShowError($"Error al generar PDF: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            AddPaymentButton.IsEnabled = !isLoading;
            VoidInvoiceButton.IsEnabled = !isLoading;
            PrintTicketButton.IsEnabled = !isLoading;
            PrintLetterButton.IsEnabled = !isLoading;
        }

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
    }
}
