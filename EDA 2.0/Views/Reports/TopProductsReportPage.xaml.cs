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
    public sealed partial class TopProductsReportPage : Page
    {
        private readonly IMediator _mediator;
        private readonly IReportPdfService _pdfService;
        private TopProductsReportData? _currentData;

        public TopProductsReportPage()
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
                var topN = int.Parse((TopNComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "10");
                var sortBy = (SortByComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Revenue";

                var result = await _mediator.Send(new GetTopProductsReportQuery
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    TopN = topN,
                    SortBy = sortBy
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

        private void UpdateUI(TopProductsReportData data)
        {
            TotalQuantityText.Text = data.TotalQuantity.ToString();
            TotalRevenueText.Text = $"L {data.TotalRevenue:N2}";

            var displayItems = data.Products.Select(i => new TopProductDisplayItem
            {
                Rank = i.Rank,
                ProductName = i.ProductName,
                FamilyName = i.FamilyName,
                QuantitySold = i.QuantitySold,
                TotalRevenue = i.TotalRevenue,
                PercentageOfTotal = i.PercentageOfTotal
            }).ToList();

            DataListView.ItemsSource = displayItems;
        }

        private async void PrintPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null) return;

            try
            {
                var pdfBytes = _pdfService.GenerateTopProductsReportPdf(_currentData);
                var tempPath = Path.Combine(Path.GetTempPath(), $"ReporteTopProductos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
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

    public class TopProductDisplayItem
    {
        public int Rank { get; set; }
        public string ProductName { get; set; } = null!;
        public string FamilyName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PercentageOfTotal { get; set; }

        public string TotalRevenueFormatted => $"L {TotalRevenue:N2}";
        public string PercentageFormatted => $"{PercentageOfTotal:N1}%";
    }
}
