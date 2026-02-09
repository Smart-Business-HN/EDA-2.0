using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.CompanyFeature.Queries;
using EDA.APPLICATION.Features.ShiftFeature.Commands.UpdateShiftCommand;
using EDA.APPLICATION.Features.ShiftFeature.Queries;
using EDA.APPLICATION.Interfaces;
using EDA.DOMAIN.Enums;
using EDA_2._0.Views.Base;
using EDA_2._0.Views.Reports;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class MainMenuPage : CancellablePage
    {
        private readonly IMediator _mediator;
        private readonly IUpdateCheckerService _updateChecker;
        private UpdateCheckResult? _updateResult;

        public MainMenuPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
            _updateChecker = App.Services.GetRequiredService<IUpdateCheckerService>();
            ConfigureMenuByRole();
            Loaded += MainMenuPage_Loaded;
        }

        private void MainMenuPage_Loaded(object sender, RoutedEventArgs e)
        {
            SafeExecuteAsync(CheckForUpdatesAsync);
        }

        private async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
        {
            try
            {
                _updateResult = await _updateChecker.CheckForUpdatesAsync(cancellationToken);

                if (cancellationToken.IsCancellationRequested) return;

                if (_updateResult.UpdateAvailable)
                {
                    UpdateInfoBar.Title = "Actualizacion disponible";
                    UpdateInfoBar.Message = $"Version {_updateResult.LatestVersion} disponible. Version actual: {_updateResult.CurrentVersion}";
                    UpdateInfoBar.IsOpen = true;
                }
            }
            catch (OperationCanceledException)
            {
                // Cancelado por navegacion
            }
            catch
            {
                // Silently ignore update check failures
            }
        }

        private void UpdateDownload_Click(object sender, RoutedEventArgs e)
        {
            if (_updateResult == null) return;

            // Prefer Store URL if available, otherwise use download URL
            var url = !string.IsNullOrEmpty(_updateResult.StoreUrl)
                ? _updateResult.StoreUrl
                : _updateResult.DownloadUrl;

            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }

        private void ConfigureMenuByRole()
        {
            var currentUser = App.CurrentUser;
            if (currentUser == null) return;

            var role = (RoleEnum)currentUser.RoleId;

            if (role == RoleEnum.Cajero)
            {
                // Cajero solo tiene acceso a: POS, Clientes, Productos
                // Ocultar items no permitidos
                NavDashboard.Visibility = Visibility.Collapsed;
                NavFamilias.Visibility = Visibility.Collapsed;
                NavImpuestos.Visibility = Visibility.Collapsed;
                NavDescuentos.Visibility = Visibility.Collapsed;
                NavProveedores.Visibility = Visibility.Collapsed;
                NavCuentasGastos.Visibility = Visibility.Collapsed;
                HeaderCompras.Visibility = Visibility.Collapsed;
                NavFacturasCompra.Visibility = Visibility.Collapsed;
                NavFacturas.Visibility = Visibility.Collapsed;
                NavTiposPago.Visibility = Visibility.Collapsed;
                NavTurnos.Visibility = Visibility.Collapsed;
                NavResumenVentas.Visibility = Visibility.Collapsed;
                NavCuentasCobrar.Visibility = Visibility.Collapsed;
                NavReportes.Visibility = Visibility.Collapsed;
                NavCierreMes.Visibility = Visibility.Collapsed;
                NavCAIs.Visibility = Visibility.Collapsed;
                NavUsuarios.Visibility = Visibility.Collapsed;
                NavEmpresa.Visibility = Visibility.Collapsed;
                NavCajas.Visibility = Visibility.Collapsed;
                NavImpresoras.Visibility = Visibility.Collapsed;
                HeaderConfiguracion.Visibility = Visibility.Collapsed;
            }
            // Admin tiene acceso a todo (por defecto visible)
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                var tag = selectedItem.Tag?.ToString();

                switch (tag)
                {
                    case "dashboard":
                        ContentFrame.Navigate(typeof(DashboardPage));
                        break;
                    case "pos":
                        ContentFrame.Navigate(typeof(POSPage));
                        break;
                    case "empresa":
                        ContentFrame.Navigate(typeof(CompanyPage));
                        break;
                    case "usuarios":
                        ContentFrame.Navigate(typeof(UsersPage));
                        break;
                    case "familias":
                        ContentFrame.Navigate(typeof(FamiliesPage));
                        break;
                    case "tipospago":
                        ContentFrame.Navigate(typeof(PaymentTypesPage));
                        break;
                    case "descuentos":
                        ContentFrame.Navigate(typeof(DiscountsPage));
                        break;
                    case "impuestos":
                        ContentFrame.Navigate(typeof(TaxesPage));
                        break;
                    case "clientes":
                        ContentFrame.Navigate(typeof(CustomersPage));
                        break;
                    case "productos":
                        ContentFrame.Navigate(typeof(ProductsPage));
                        break;
                    case "cais":
                        ContentFrame.Navigate(typeof(CaisPage));
                        break;
                    case "facturas":
                        ContentFrame.Navigate(typeof(InvoicesPage));
                        break;
                    case "turnos":
                        ContentFrame.Navigate(typeof(ShiftsPage));
                        break;
                    case "resumenes":
                        ContentFrame.Navigate(typeof(SalesSummaryPage));
                        break;
                    case "report_sales_period":
                        ContentFrame.Navigate(typeof(SalesByPeriodReportPage));
                        break;
                    case "report_payment_methods":
                        ContentFrame.Navigate(typeof(PaymentMethodsReportPage));
                        break;
                    case "report_taxes":
                        ContentFrame.Navigate(typeof(TaxSummaryReportPage));
                        break;
                    case "report_low_stock":
                        ContentFrame.Navigate(typeof(LowStockReportPage));
                        break;
                    case "report_expiring":
                        ContentFrame.Navigate(typeof(ExpiringProductsReportPage));
                        break;
                    case "report_top_products":
                        ContentFrame.Navigate(typeof(TopProductsReportPage));
                        break;
                    case "report_inventory":
                        ContentFrame.Navigate(typeof(InventoryReportPage));
                        break;
                    case "cierre_mes":
                        ContentFrame.Navigate(typeof(MonthlyClosingReportPage));
                        break;
                    case "cuentascobrar":
                        ContentFrame.Navigate(typeof(ReceivablesPage));
                        break;
                    case "proveedores":
                        ContentFrame.Navigate(typeof(ProvidersPage));
                        break;
                    case "cuentasgastos":
                        ContentFrame.Navigate(typeof(ExpenseAccountsPage));
                        break;
                    case "facturascompra":
                        ContentFrame.Navigate(typeof(PurchaseBillsPage));
                        break;
                    case "cajas":
                        ContentFrame.Navigate(typeof(CashRegistersPage));
                        break;
                    case "impresoras":
                        ContentFrame.Navigate(typeof(PrinterConfigPage));
                        break;
                }
            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // ItemInvoked se dispara siempre, incluso con SelectsOnInvoked="False"
            if (args.InvokedItemContainer is NavigationViewItem invokedItem)
            {
                var tag = invokedItem.Tag?.ToString();

                // Si es el item de configuracion de usuario, mostrar flyout
                if (tag == "usersettings")
                {
                    FlyoutBase.ShowAttachedFlyout(invokedItem);
                }
            }
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Cambiar Contrasena",
                Content = "Esta funcionalidad sera implementada proximamente.",
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };
        }

        private async void CloseShift_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentShift == null || !App.CurrentShift.IsOpen)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "No tiene un turno abierto.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                return;
            }

            var shift = App.CurrentShift;
            var ct = PageCancellationToken;

            // Obtener datos de cierre del turno via Query
            var closingResult = await _mediator.Send(new GetShiftClosingDataQuery
            {
                UserId = shift.UserId,
                ShiftStartTime = shift.StartTime,
                InitialAmount = shift.InitialAmount
            }, ct);

            if (!IsPageActive) return;

            if (!closingResult.Succeeded || closingResult.Data == null)
            {
                var errDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Error al obtener datos del turno.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.XamlRoot
                };
                await errDialog.ShowAsync();
                return;
            }

            var closingData = closingResult.Data;

            var infoText = new TextBlock
            {
                Text = $"Turno: {shift.ShiftType}\nInicio: {shift.StartTime:dd/MM/yyyy HH:mm}\nSaldo inicial: L {shift.InitialAmount:N2}\n\nVentas en efectivo: L {closingData.ExpectedCash:N2}\nVentas en tarjeta: L {closingData.ExpectedCard:N2}\nSaldo esperado: L {closingData.ExpectedTotal:N2}",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var cashBox = new NumberBox
            {
                Header = "Efectivo en caja *",
                PlaceholderText = "0.00",
                Value = double.NaN,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var cardBox = new NumberBox
            {
                Header = "Total en tarjeta *",
                PlaceholderText = "0.00",
                Value = double.NaN,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var content = new StackPanel
            {
                Width = 400,
                Children = { infoText, cashBox, cardBox }
            };

            var dialog = new ContentDialog
            {
                Title = "Cerrar Turno",
                Content = content,
                PrimaryButtonText = "Cerrar Turno",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Verificar facturas pendientes de imprimir (EndOfDay strategy)
                if (closingData.UnprintedInvoices.Count > 0)
                {
                    var unprintedInfoText = new TextBlock
                    {
                        Text = $"Hay {closingData.UnprintedInvoices.Count} factura(s) pendientes de imprimir:",
                        Margin = new Thickness(0, 0, 0, 12),
                        TextWrapping = TextWrapping.Wrap
                    };

                    var unprintedList = new ListView
                    {
                        ItemsSource = closingData.UnprintedInvoices.Select(i => $"{i.InvoiceNumber} - {i.Customer?.Name ?? "Cliente"} - L {i.Total:N2}").ToList(),
                        MaxHeight = 200,
                        SelectionMode = ListViewSelectionMode.None
                    };

                    var printContent = new StackPanel
                    {
                        Width = 450,
                        Children = { unprintedInfoText, unprintedList }
                    };

                    var printDialog = new ContentDialog
                    {
                        Title = "Facturas Pendientes de Imprimir",
                        Content = printContent,
                        PrimaryButtonText = "Continuar sin imprimir",
                        CloseButtonText = "Cancelar cierre",
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = this.XamlRoot
                    };

                    var printResult = await printDialog.ShowAsync();
                    if (printResult != ContentDialogResult.Primary)
                    {
                        return; // Cancelar cierre de turno
                    }
                }

                try
                {
                    decimal finalCash = double.IsNaN(cashBox.Value) ? 0 : (decimal)cashBox.Value;
                    decimal finalCard = double.IsNaN(cardBox.Value) ? 0 : (decimal)cardBox.Value;
                    decimal finalAmount = finalCash + finalCard + shift.InitialAmount;
                    decimal difference = closingData.ExpectedTotal - finalAmount;

                    var command = new UpdateShiftCommand
                    {
                        Id = shift.Id,
                        FinalCashAmount = finalCash,
                        FinalCardAmount = finalCard,
                        ExpectedAmount = closingData.ExpectedTotal
                    };

                    var updateResult = await _mediator.Send(command, ct);

                    if (!IsPageActive) return;

                    if (updateResult.Succeeded)
                    {
                        // Obtener datos de empresa
                        var companyResult = await _mediator.Send(new GetCompanyQuery(), ct);

                        if (!IsPageActive) return;
                        var company = companyResult.Data;

                        // Generar PDF
                        var pdfService = App.Services.GetRequiredService<IShiftReportPdfService>();
                        var reportData = new ShiftReportData
                        {
                            UserName = App.CurrentUser?.Name ?? "Usuario",
                            ShiftType = shift.ShiftType,
                            StartTime = shift.StartTime,
                            EndTime = DateTime.Now,
                            InitialAmount = shift.InitialAmount,
                            FinalCashAmount = finalCash,
                            FinalCardAmount = finalCard,
                            FinalAmount = finalAmount,
                            ExpectedCash = closingData.ExpectedCash,
                            ExpectedCard = closingData.ExpectedCard,
                            ExpectedAmount = closingData.ExpectedTotal,
                            Difference = difference,
                            TotalInvoices = closingData.TotalInvoices,
                            TotalSales = closingData.TotalSales,
                            CompanyName = company?.Name ?? "Empresa",
                            CompanyAddress = company?.Address1,
                            CompanyRTN = company?.RTN
                        };

                        var pdfBytes = pdfService.GenerateShiftReportPdf(reportData);
                        var tempPath = Path.Combine(Path.GetTempPath(), $"ReporteTurno_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                        await File.WriteAllBytesAsync(tempPath, pdfBytes);
                        Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });

                        App.CurrentShift = null;

                        var successDialog = new ContentDialog
                        {
                            Title = "Turno Cerrado",
                            Content = $"Turno cerrado exitosamente.\nSaldo esperado: L {closingData.ExpectedTotal:N2}\nSaldo reportado: L {finalAmount:N2}\nDiferencia: L {difference:N2}\nFacturas: {closingData.TotalInvoices}\nTotal Ventas: L {closingData.TotalSales:N2}",
                            CloseButtonText = "Aceptar",
                            XamlRoot = this.XamlRoot
                        };
                        await successDialog.ShowAsync();

                        // Redirigir a login
                        App.CurrentUser = null;
                        App.MainWindow.NavigateToPage(typeof(LoginPage));
                    }
                }
                catch (Exception ex)
                {
                    var errDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Error al cerrar turno: {ex.Message}",
                        CloseButtonText = "Aceptar",
                        XamlRoot = this.XamlRoot
                    };
                    await errDialog.ShowAsync();
                }
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            App.CurrentShift = null;
            App.MainWindow.NavigateToPage(typeof(LoginPage));
        }

        public void NavigateToInvoiceDetail(int invoiceId)
        {
            var detailPage = new InvoiceDetailPage();
            detailPage.SetInvoiceId(invoiceId);
            ContentFrame.Navigate(typeof(InvoiceDetailPage), invoiceId);
            ContentFrame.Content = detailPage;
        }

        public void NavigateToInvoiceCreate()
        {
            ContentFrame.Navigate(typeof(InvoiceCreatePage));
        }

        public void NavigateToInvoices()
        {
            ContentFrame.Navigate(typeof(InvoicesPage));
            NavView.SelectedItem = NavFacturas;
        }

        public void NavigateToReceivables()
        {
            ContentFrame.Navigate(typeof(ReceivablesPage));
            NavView.SelectedItem = NavCuentasCobrar;
        }

        public void NavigateToCustomerReceivablesDetail(int customerId)
        {
            var detailPage = new CustomerReceivablesDetailPage();
            detailPage.SetCustomerId(customerId);
            ContentFrame.Navigate(typeof(CustomerReceivablesDetailPage), customerId);
            ContentFrame.Content = detailPage;
        }

        public void NavigateToPurchaseBills()
        {
            ContentFrame.Navigate(typeof(PurchaseBillsPage));
            NavView.SelectedItem = NavFacturasCompra;
        }

        public void NavigateToPurchaseBillDetail(int purchaseBillId)
        {
            var detailPage = new PurchaseBillDetailPage();
            detailPage.SetPurchaseBillId(purchaseBillId);
            ContentFrame.Navigate(typeof(PurchaseBillDetailPage), purchaseBillId);
            ContentFrame.Content = detailPage;
        }

        public void NavigateToPurchaseBillEdit(int purchaseBillId)
        {
            // Navigate to PurchaseBillsPage which will handle the edit
            var page = new PurchaseBillsPage();
            page.SetEditPurchaseBillId(purchaseBillId);
            ContentFrame.Navigate(typeof(PurchaseBillsPage), purchaseBillId);
            ContentFrame.Content = page;
            NavView.SelectedItem = NavFacturasCompra;
        }
    }
}
