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

            var infoText = new TextBlock
            {
                Text = $"Turno: {shift.ShiftType}\nInicio: {shift.StartTime:dd/MM/yyyy HH:mm}\nMonto inicial: L {shift.InitialAmount:N2}",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var finalAmountBox = new NumberBox
            {
                Header = "Monto final en caja *",
                PlaceholderText = "0.00",
                Value = double.NaN,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var content = new StackPanel
            {
                Width = 400,
                Children = { infoText, finalAmountBox }
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
                    decimal finalAmount = double.IsNaN(finalAmountBox.Value) ? 0 : (decimal)finalAmountBox.Value;

                    var command = new UpdateShiftCommand
                    {
                        Id = shift.Id,
                        FinalAmount = finalAmount
                    };

                    var updateResult = await mediator.Send(command);

                    if (updateResult.Succeeded)
                    {
                        // Consultar facturas del turno
                        var dbContext = App.Services.GetRequiredService<DatabaseContext>();
                        var invoices = await dbContext.Invoices
                            .Where(i => i.UserId == shift.UserId && i.Date >= shift.StartTime)
                            .ToListAsync();

                        var totalInvoices = invoices.Count;
                        var totalSales = invoices.Sum(i => i.Total);

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
                            FinalAmount = finalAmount,
                            Difference = finalAmount - shift.InitialAmount,
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
                            Content = $"Turno cerrado exitosamente.\nDiferencia: L {(finalAmount - shift.InitialAmount):N2}\nFacturas: {totalInvoices}\nTotal Ventas: L {totalSales:N2}",
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
