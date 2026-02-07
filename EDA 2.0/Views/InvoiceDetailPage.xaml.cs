using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.CompanyFeature.Queries;
using EDA.APPLICATION.Features.InvoiceFeature.Commands.AddPaymentToInvoiceCommand;
using EDA.APPLICATION.Features.InvoiceFeature.Commands.VoidInvoiceCommand;
using EDA.APPLICATION.Features.InvoiceFeature.Queries.GetInvoiceByIdQuery;
using EDA.APPLICATION.Features.PaymentTypeFeature.Queries;
using EDA.APPLICATION.Interfaces;
using EDA.DOMAIN.Entities;
using EDA.DOMAIN.Enums;
using MediatR;
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
using Windows.UI;

namespace EDA_2._0.Views
{
    public sealed partial class InvoiceDetailPage : Page
    {
        private readonly IMediator _mediator;
        private int _invoiceId;
        private Invoice? _invoice;
        private List<PaymentType> _paymentTypes = new();

        public InvoiceDetailPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        public void SetInvoiceId(int invoiceId)
        {
            _invoiceId = invoiceId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPaymentTypes();
            await LoadInvoice();
        }

        private async Task LoadPaymentTypes()
        {
            try
            {
                var result = await _mediator.Send(new GetAllPaymentTypesQuery { GetAll = true });
                if (result.Succeeded && result.Data != null)
                {
                    _paymentTypes = result.Data.Items.ToList();
                }
            }
            catch
            {
                // Silently ignore payment types loading errors
            }
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

            UpdateStatusBadge();
        }

        private void UpdateStatusBadge()
        {
            if (_invoice == null) return;

            string statusName;
            Color badgeColor;

            switch (_invoice.Status)
            {
                case (int)InvoiceStatusEnum.Created:
                    statusName = "Creada";
                    badgeColor = Color.FromArgb(255, 52, 152, 219); // Azul
                    AddPaymentButton.Visibility = Visibility.Visible;
                    VoidInvoiceButton.Visibility = Visibility.Visible;
                    break;
                case (int)InvoiceStatusEnum.Paid:
                    statusName = "Pagada";
                    badgeColor = Color.FromArgb(255, 39, 174, 96); // Verde
                    AddPaymentButton.Visibility = Visibility.Collapsed;
                    VoidInvoiceButton.Visibility = Visibility.Visible;
                    break;
                case (int)InvoiceStatusEnum.Cancelled:
                    statusName = "Anulada";
                    badgeColor = Color.FromArgb(255, 231, 76, 60); // Rojo
                    AddPaymentButton.Visibility = Visibility.Collapsed;
                    VoidInvoiceButton.Visibility = Visibility.Collapsed;
                    break;
                default:
                    statusName = "Desconocido";
                    badgeColor = Color.FromArgb(255, 128, 128, 128); // Gris
                    AddPaymentButton.Visibility = Visibility.Collapsed;
                    VoidInvoiceButton.Visibility = Visibility.Collapsed;
                    break;
            }

            StatusText.Text = statusName;
            StatusBadge.Background = new SolidColorBrush(badgeColor);
            StatusText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.MainWindow as MainWindow;
            mainWindow?.NavigateToInvoices();
        }

        private async void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_invoice == null) return;

            var paymentTypeCombo = new ComboBox
            {
                PlaceholderText = "Tipo de pago",
                DisplayMemberPath = "Name",
                ItemsSource = _paymentTypes,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var amountBox = new NumberBox
            {
                Header = $"Monto (Pendiente: L {_invoice.OutstandingAmount:N2})",
                PlaceholderText = "0.00",
                Value = (double)_invoice.OutstandingAmount,
                Minimum = 0.01,
                Maximum = (double)_invoice.OutstandingAmount,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var content = new StackPanel { Width = 300, Spacing = 12, Children = { paymentTypeCombo, amountBox } };

            var dialog = new ContentDialog
            {
                Title = "Agregar Pago",
                Content = content,
                PrimaryButtonText = "Agregar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary &&
                paymentTypeCombo.SelectedItem is PaymentType paymentType &&
                !double.IsNaN(amountBox.Value) && amountBox.Value > 0)
            {
                SetLoading(true);

                try
                {
                    var command = new AddPaymentToInvoiceCommand
                    {
                        InvoiceId = _invoiceId,
                        PaymentTypeId = paymentType.Id,
                        Amount = (decimal)amountBox.Value
                    };

                    var addResult = await _mediator.Send(command);
                    if (addResult.Succeeded)
                    {
                        await LoadInvoice();
                        await ShowSuccess("Pago agregado exitosamente.");
                    }
                    else
                    {
                        await ShowError(addResult.Message ?? "Error al agregar pago.");
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
        }

        private async void VoidInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_invoice == null) return;

            var confirmDialog = new ContentDialog
            {
                Title = "Confirmar Anulacion",
                Content = "¿Esta seguro que desea anular esta factura? Esta accion no se puede deshacer.",
                PrimaryButtonText = "Anular",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SetLoading(true);

                try
                {
                    var command = new VoidInvoiceCommand { InvoiceId = _invoiceId };
                    var voidResult = await _mediator.Send(command);

                    if (voidResult.Succeeded)
                    {
                        await LoadInvoice();
                        await ShowSuccess("Factura anulada exitosamente.");
                    }
                    else
                    {
                        await ShowError(voidResult.Message ?? "Error al anular factura.");
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
                var companyResult = await _mediator.Send(new GetCompanyQuery());
                var company = companyResult.Data ?? new Company
                {
                    Name = "Mi Empresa",
                    Owner = "Propietario"
                };

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
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
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
