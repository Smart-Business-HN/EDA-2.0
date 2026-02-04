using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.ReportFeature.Queries;
using EDA.APPLICATION.Interfaces;
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

namespace EDA_2._0.Views.Reports
{
    public sealed partial class LowStockReportPage : Page
    {
        private readonly IMediator _mediator;
        private readonly IReportPdfService _pdfService;
        private LowStockReportData? _currentData;

        public LowStockReportPage()
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
                var result = await _mediator.Send(new GetLowStockReportQuery());

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

        private void UpdateUI(LowStockReportData data)
        {
            TotalAtRiskText.Text = data.TotalProductsAtRisk.ToString();
            TotalOutOfStockText.Text = data.TotalOutOfStock.ToString();

            var displayItems = data.Products.Select(i => new LowStockDisplayItem
            {
                ProductName = i.ProductName,
                FamilyName = i.FamilyName,
                CurrentStock = i.CurrentStock,
                MinStock = i.MinStock,
                SuggestedOrder = i.SuggestedOrder,
                Status = i.Status
            }).ToList();

            DataListView.ItemsSource = displayItems;
        }

        private async void PrintPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null) return;

            try
            {
                var pdfBytes = _pdfService.GenerateLowStockReportPdf(_currentData);
                var tempPath = Path.Combine(Path.GetTempPath(), $"ReporteStockBajo_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
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

    public class LowStockDisplayItem
    {
        public string ProductName { get; set; } = null!;
        public string FamilyName { get; set; } = null!;
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
        public int SuggestedOrder { get; set; }
        public string Status { get; set; } = null!;

        public Brush StatusBackground => Status switch
        {
            "Sin Stock" => new SolidColorBrush(Microsoft.UI.Colors.Red),
            "Critico" => new SolidColorBrush(Microsoft.UI.Colors.Orange),
            _ => new SolidColorBrush(Microsoft.UI.Colors.Gold)
        };
    }
}
