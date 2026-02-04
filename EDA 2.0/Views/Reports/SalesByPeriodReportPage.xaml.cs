using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.ReportFeature.Queries;
using EDA.APPLICATION.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EDA_2._0.Views.Reports
{
    public sealed partial class SalesByPeriodReportPage : Page
    {
        private readonly IMediator _mediator;
        private readonly IReportPdfService _pdfService;
        private SalesByPeriodReportData? _currentData;

        public SalesByPeriodReportPage()
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
                var grouping = (GroupingComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Day";

                var result = await _mediator.Send(new GetSalesByPeriodReportQuery
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    GroupingType = grouping
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

        private void UpdateUI(SalesByPeriodReportData data)
        {
            TotalInvoicesText.Text = data.TotalInvoices.ToString();
            TotalSalesText.Text = $"L {data.GrandTotal:N2}";
            TotalTaxesText.Text = $"L {data.TotalTaxes:N2}";

            var displayItems = data.Items.Select(i => new SalesPeriodDisplayItem
            {
                PeriodLabel = i.PeriodLabel,
                InvoiceCount = i.InvoiceCount,
                Subtotal = i.Subtotal,
                TotalTaxes = i.TotalTaxes,
                Total = i.Total
            }).ToList();

            DataListView.ItemsSource = displayItems;
        }

        private async void PrintPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null) return;

            try
            {
                var pdfBytes = _pdfService.GenerateSalesByPeriodReportPdf(_currentData);
                var tempPath = Path.Combine(Path.GetTempPath(), $"ReporteVentasPeriodo_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
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

    public class SalesPeriodDisplayItem
    {
        public string PeriodLabel { get; set; } = null!;
        public int InvoiceCount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal Total { get; set; }
        public string SubtotalFormatted => $"L {Subtotal:N2}";
        public string TotalTaxesFormatted => $"L {TotalTaxes:N2}";
        public string TotalFormatted => $"L {Total:N2}";
    }
}
