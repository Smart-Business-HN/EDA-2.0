using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.CompanyFeature.Queries;
using EDA.APPLICATION.Features.ReceivablesFeature.Queries.GetCustomerReceivablesDetailQuery;
using EDA.APPLICATION.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace EDA_2._0.Views
{
    public sealed partial class CustomerReceivablesDetailPage : Page
    {
        private readonly IMediator _mediator;
        private int _customerId;
        private CustomerReceivablesDetail? _detail;

        public CustomerReceivablesDetailPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        public void SetCustomerId(int customerId)
        {
            _customerId = customerId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomerDetail();
        }

        private async Task LoadCustomerDetail()
        {
            SetLoading(true);

            try
            {
                var result = await _mediator.Send(new GetCustomerReceivablesDetailQuery
                {
                    CustomerId = _customerId
                });

                if (result.Succeeded && result.Data != null)
                {
                    _detail = result.Data;
                    PopulateData();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar detalle del cliente");
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

        private void PopulateData()
        {
            if (_detail == null) return;

            // Header
            CustomerNameText.Text = _detail.CustomerName;
            CustomerRtnText.Text = $"RTN: {_detail.CustomerRtn ?? "-"}";

            // Summary Cards
            TotalOwedText.Text = $"L {_detail.TotalOwed:N2}";
            PendingInvoicesText.Text = _detail.PendingInvoicesCount.ToString();
            OverdueAmountText.Text = $"L {_detail.OverdueAmount:N2}";
            DueIn7DaysText.Text = $"L {_detail.DueIn7DaysAmount:N2}";

            // Invoices list
            InvoicesListView.ItemsSource = _detail.PendingInvoices;
        }

        private void InvoicesListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Item is PendingInvoiceItem invoice)
            {
                // Buscar el TextBlock de dias vencidos en el item
                args.RegisterUpdateCallback((s, a) =>
                {
                    if (a.ItemContainer.ContentTemplateRoot is Grid grid)
                    {
                        var daysText = grid.FindName("DaysOverdueText") as TextBlock;
                        if (daysText != null)
                        {
                            if (invoice.IsOverdue)
                            {
                                daysText.Text = $"+{invoice.DaysOverdue}";
                                daysText.Foreground = new SolidColorBrush(Color.FromArgb(255, 231, 76, 60));
                            }
                            else if (invoice.DaysOverdue < 0)
                            {
                                daysText.Text = invoice.DaysOverdue.ToString();
                                daysText.Foreground = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96));
                            }
                            else
                            {
                                daysText.Text = "0";
                                daysText.Foreground = new SolidColorBrush(Color.FromArgb(255, 243, 156, 18));
                            }
                        }
                    }
                });

                // Colorear fila si esta vencida
                if (invoice.IsOverdue)
                {
                    args.ItemContainer.Background = new SolidColorBrush(Color.FromArgb(25, 231, 76, 60));
                }
                else
                {
                    args.ItemContainer.Background = null;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.MainWindow as MainWindow;
            mainWindow?.NavigateToReceivables();
        }

        private void ViewInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PendingInvoiceItem invoice)
            {
                var mainWindow = App.MainWindow as MainWindow;
                mainWindow?.NavigateToInvoiceDetail(invoice.InvoiceId);
            }
        }

        private async void GenerateStatementButton_Click(object sender, RoutedEventArgs e)
        {
            if (_detail == null) return;

            SetLoading(true);

            try
            {
                // Obtener datos de la empresa
                var companyResult = await _mediator.Send(new GetCompanyQuery());
                var company = companyResult.Data;

                // Construir datos para el PDF
                var pdfData = new CustomerStatementPdfData
                {
                    CompanyName = company?.Name ?? "Mi Empresa",
                    CompanyRtn = company?.RTN,
                    CompanyAddress = company?.Address1,
                    CompanyPhone = company?.PhoneNumber1,
                    CompanyEmail = company?.Email,
                    CompanyLogo = company?.Logo,
                    CustomerName = _detail.CustomerName,
                    CustomerRtn = _detail.CustomerRtn,
                    CustomerCompany = _detail.CustomerCompany,
                    GeneratedAt = DateTime.Now,
                    TotalOwed = _detail.TotalOwed,
                    PendingInvoicesCount = _detail.PendingInvoicesCount,
                    OverdueAmount = _detail.OverdueAmount,
                    Invoices = _detail.PendingInvoices.Select(inv => new StatementInvoiceItem
                    {
                        InvoiceNumber = inv.InvoiceNumber,
                        IssueDate = inv.IssueDate,
                        DueDate = inv.DueDate,
                        Total = inv.Total,
                        OutstandingAmount = inv.OutstandingAmount,
                        DaysOverdue = inv.DaysOverdue
                    }).ToList()
                };

                // Generar PDF
                var pdfService = App.Services.GetRequiredService<ICustomerStatementPdfService>();
                var pdfBytes = pdfService.GenerateCustomerStatementPdf(pdfData);

                // Guardar y abrir
                var fileName = $"EstadoCuenta_{_detail.CustomerName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var tempPath = Path.Combine(Path.GetTempPath(), fileName);
                await File.WriteAllBytesAsync(tempPath, pdfBytes);

                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
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
            GenerateStatementButton.IsEnabled = !isLoading;
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
                            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
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
