using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.ShiftFeature.Commands.UpdateShiftCommand;
using EDA.APPLICATION.Interfaces;
using EDA.DOMAIN.Enums;
using EDA.INFRAESTRUCTURE;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EDA_2._0.Views
{
    public sealed partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
            ConfigureMenuByRole();
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
                NavFacturas.Visibility = Visibility.Collapsed;
                NavTiposPago.Visibility = Visibility.Collapsed;
                NavTurnos.Visibility = Visibility.Collapsed;
                NavResumenVentas.Visibility = Visibility.Collapsed;
                NavCAIs.Visibility = Visibility.Collapsed;
                NavUsuarios.Visibility = Visibility.Collapsed;
                NavEmpresa.Visibility = Visibility.Collapsed;
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

            // Consultar pagos del turno para mostrar info
            var dbContext = App.Services.GetRequiredService<DatabaseContext>();
            var invoiceIds = await dbContext.Invoices
                .Where(i => i.UserId == shift.UserId && i.Date >= shift.StartTime)
                .Select(i => i.Id)
                .ToListAsync();

            var payments = await dbContext.InvoicePayments
                .Where(p => invoiceIds.Contains(p.InvoiceId))
                .Include(p => p.PaymentType)
                .ToListAsync();

            // Efectivo = PaymentTypeId 1, Tarjeta = PaymentTypeId 3, Transferencia = 2
            var expectedCash = payments.Where(p => p.PaymentTypeId == 1).Sum(p => p.Amount);
            var expectedCard = payments.Where(p => p.PaymentTypeId == 3 || p.PaymentTypeId == 2).Sum(p => p.Amount);
            var expectedTotal = shift.InitialAmount + expectedCash + expectedCard;

            var totalInvoices = invoiceIds.Count;
            var totalSales = await dbContext.Invoices
                .Where(i => invoiceIds.Contains(i.Id))
                .SumAsync(i => i.Total);

            var infoText = new TextBlock
            {
                Text = $"Turno: {shift.ShiftType}\nInicio: {shift.StartTime:dd/MM/yyyy HH:mm}\nSaldo inicial: L {shift.InitialAmount:N2}\n\nVentas en efectivo: L {expectedCash:N2}\nVentas en tarjeta: L {expectedCard:N2}\nSaldo esperado: L {expectedTotal:N2}",
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
                try
                {
                    var mediator = App.Services.GetRequiredService<IMediator>();
                    decimal finalCash = double.IsNaN(cashBox.Value) ? 0 : (decimal)cashBox.Value;
                    decimal finalCard = double.IsNaN(cardBox.Value) ? 0 : (decimal)cardBox.Value;
                    decimal finalAmount = finalCash + finalCard + shift.InitialAmount;
                    decimal difference = expectedTotal - finalAmount;

                    var command = new UpdateShiftCommand
                    {
                        Id = shift.Id,
                        FinalCashAmount = finalCash,
                        FinalCardAmount = finalCard,
                        ExpectedAmount = expectedTotal
                    };

                    var updateResult = await mediator.Send(command);

                    if (updateResult.Succeeded)
                    {
                        // Obtener datos de empresa
                        var company = await dbContext.Companies.FirstOrDefaultAsync();

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
                            ExpectedCash = expectedCash,
                            ExpectedCard = expectedCard,
                            ExpectedAmount = expectedTotal,
                            Difference = difference,
                            TotalInvoices = totalInvoices,
                            TotalSales = totalSales,
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
                            Content = $"Turno cerrado exitosamente.\nSaldo esperado: L {expectedTotal:N2}\nSaldo reportado: L {finalAmount:N2}\nDiferencia: L {difference:N2}\nFacturas: {totalInvoices}\nTotal Ventas: L {totalSales:N2}",
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

        public void NavigateToInvoices()
        {
            ContentFrame.Navigate(typeof(InvoicesPage));
            NavView.SelectedItem = NavFacturas;
        }
    }
}
