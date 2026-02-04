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
    public sealed partial class InventoryReportPage : Page
    {
        private readonly IMediator _mediator;
        private readonly IReportPdfService _pdfService;
        private InventoryReportData? _currentData;

        public InventoryReportPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
            _pdfService = App.Services.GetRequiredService<IReportPdfService>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            LoadingRing.IsActive = true;
            PrintPdfButton.IsEnabled = false;

            try
            {
                var result = await _mediator.Send(new GetInventoryReportQuery());

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

        private void UpdateUI(InventoryReportData data)
        {
            TotalProductsText.Text = data.TotalProducts.ToString();
            TotalUnitsText.Text = data.TotalUnits.ToString();
            TotalValueText.Text = $"L {data.TotalInventoryValue:N2}";

            var displayItems = data.FamilyGroups.Select(g => new InventoryFamilyDisplayItem
            {
                FamilyName = g.FamilyName,
                TotalProducts = g.TotalProducts,
                TotalUnits = g.TotalUnits,
                TotalValue = g.TotalValue,
                Products = g.Products.Select(p => new InventoryProductDisplayItem
                {
                    ProductName = p.ProductName,
                    Barcode = p.Barcode ?? "-",
                    Stock = p.Stock,
                    Price = p.Price,
                    TotalValue = p.TotalValue
                }).ToList()
            }).ToList();

            DataListView.ItemsSource = displayItems;
        }

        private async void PrintPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null) return;

            try
            {
                var pdfBytes = _pdfService.GenerateInventoryReportPdf(_currentData);
                var tempPath = Path.Combine(Path.GetTempPath(), $"ReporteInventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
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

    public class InventoryFamilyDisplayItem
    {
        public string FamilyName { get; set; } = null!;
        public int TotalProducts { get; set; }
        public int TotalUnits { get; set; }
        public decimal TotalValue { get; set; }
        public List<InventoryProductDisplayItem> Products { get; set; } = new();

        public string TotalUnitsFormatted => $"{TotalUnits} uds";
        public string TotalValueFormatted => $"L {TotalValue:N2}";
    }

    public class InventoryProductDisplayItem
    {
        public string ProductName { get; set; } = null!;
        public string Barcode { get; set; } = null!;
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public decimal TotalValue { get; set; }

        public string PriceFormatted => $"L {Price:N2}";
        public string TotalValueFormatted => $"L {TotalValue:N2}";
    }
}
