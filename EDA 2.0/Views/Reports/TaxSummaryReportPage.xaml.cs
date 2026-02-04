using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.ReportFeature.Queries;
using EDA.APPLICATION.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace EDA_2._0.Views.Reports
{
    public sealed partial class TaxSummaryReportPage : Page
    {
        private readonly IMediator _mediator;
        private readonly IReportPdfService _pdfService;
        private TaxSummaryReportData? _currentData;

        public TaxSummaryReportPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
            _pdfService = App.Services.GetRequiredService<IReportPdfService>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            FromDatePicker.Date = new DateTimeOffset(new DateTime(today.Year, today.Month, 1));
            ToDatePicker.Date = new DateTimeOffset(today);

            await LoadDataAsync();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            LoadingRing.IsActive = true;
            PrintPdfButton.IsEnabled = false;

            try
            {
                var fromDate = FromDatePicker.Date.DateTime.Date;
                var toDate = ToDatePicker.Date.DateTime.Date.AddDays(1);

                var result = await _mediator.Send(new GetTaxSummaryReportQuery
                {
                    FromDate = fromDate,
                    ToDate = toDate
                });

                if (result.Succeeded && result.Data != null)
                {
                    _currentData = result.Data;
                    UpdateUI(_currentData);
                    PrintPdfButton.IsEnabled = true;
                }
                else
                {
                    await ShowErrorDialog(result.Message ?? "Error al cargar datos");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog(ex.Message);
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
        }

        private void UpdateUI(TaxSummaryReportData data)
        {
            TotalInvoicesText.Text = data.TotalInvoices.ToString();
            TotalSalesText.Text = $"L {data.GrandTotalSales:N2}";
            TotalTaxesText.Text = $"L {data.GrandTotalTaxes:N2}";

            TaxedAt15Text.Text = $"L {data.TotalTaxedAt15:N2}";
            TaxesAt15Text.Text = $"L {data.TotalTaxesAt15:N2}";
            TaxedAt18Text.Text = $"L {data.TotalTaxedAt18:N2}";
            TaxesAt18Text.Text = $"L {data.TotalTaxesAt18:N2}";
            ExemptText.Text = $"L {data.TotalExempt:N2}";
        }

        private async void PrintPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null) return;

            try
            {
                var pdfBytes = _pdfService.GenerateTaxSummaryReportPdf(_currentData);
                var tempPath = Path.Combine(Path.GetTempPath(), $"ReporteImpuestos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                await File.WriteAllBytesAsync(tempPath, pdfBytes);
                Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error al generar PDF: {ex.Message}");
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
