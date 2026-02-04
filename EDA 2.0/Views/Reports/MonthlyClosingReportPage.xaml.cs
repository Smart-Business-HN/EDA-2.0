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
    public sealed partial class MonthlyClosingReportPage : Page
    {
        private readonly IMediator _mediator;
        private readonly IReportPdfService _pdfService;
        private MonthlyClosingReportData? _currentData;

        public MonthlyClosingReportPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
            _pdfService = App.Services.GetRequiredService<IReportPdfService>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Set current month/year as default
            var now = DateTime.Now;
            YearNumberBox.Value = now.Year;
            MonthComboBox.SelectedIndex = now.Month - 1;
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (MonthComboBox.SelectedItem is not ComboBoxItem selectedMonth)
                return;

            var month = int.Parse(selectedMonth.Tag?.ToString() ?? "1");
            var year = (int)YearNumberBox.Value;

            LoadingRing.IsActive = true;
            PrintPdfButton.IsEnabled = false;

            try
            {
                var result = await _mediator.Send(new GetMonthlyClosingReportQuery
                {
                    Year = year,
                    Month = month
                });

                if (result.Succeeded && result.Data != null)
                {
                    _currentData = result.Data;
                    UpdateUI(_currentData);
                    PrintPdfButton.IsEnabled = true;
                }
                else
                {
                    await ShowErrorDialog(result.Message ?? "Error al generar cierre");
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

        private void UpdateUI(MonthlyClosingReportData data)
        {
            // Summary cards
            TotalInvoicesText.Text = data.TotalInvoices.ToString();
            TotalSalesText.Text = $"L {data.TotalSales:N2}";
            TotalTaxesText.Text = $"L {data.GrandTotalTaxes:N2}";
            TotalDiscountsText.Text = $"L {data.TotalDiscounts:N2}";

            // Tax breakdown
            TaxedAt15Text.Text = $"L {data.TotalTaxedAt15:N2}";
            TaxesAt15Text.Text = $"L {data.TotalTaxesAt15:N2}";
            TaxedAt18Text.Text = $"L {data.TotalTaxedAt18:N2}";
            TaxesAt18Text.Text = $"L {data.TotalTaxesAt18:N2}";
            ExemptText.Text = $"L {data.TotalExempt:N2}";

            // Totals
            var totalTaxed = data.TotalTaxedAt15 + data.TotalTaxedAt18 + data.TotalExempt;
            TotalTaxedText.Text = $"L {totalTaxed:N2}";
            GrandTotalTaxesText.Text = $"L {data.GrandTotalTaxes:N2}";

            // Period info
            PeriodText.Text = $"Periodo: {data.PeriodStart:dd/MM/yyyy} - {data.PeriodEnd:dd/MM/yyyy}";
            GeneratedAtText.Text = $"Generado: {data.GeneratedAt:dd/MM/yyyy HH:mm:ss}";
            PeriodInfoCard.Visibility = Visibility.Visible;
        }

        private async void PrintPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null) return;

            try
            {
                var pdfBytes = _pdfService.GenerateMonthlyClosingReportPdf(_currentData);
                var tempPath = Path.Combine(Path.GetTempPath(), $"CierreMes_{_currentData.MonthName}_{_currentData.Year}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
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
