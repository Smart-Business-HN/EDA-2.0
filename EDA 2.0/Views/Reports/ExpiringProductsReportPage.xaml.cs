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
    public sealed partial class ExpiringProductsReportPage : Page
    {
        private readonly IMediator _mediator;
        private readonly IReportPdfService _pdfService;
        private ExpiringProductsReportData? _currentData;

        public ExpiringProductsReportPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
            _pdfService = App.Services.GetRequiredService<IReportPdfService>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
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
                var daysThreshold = int.Parse((DaysComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "30");

                var result = await _mediator.Send(new GetExpiringProductsReportQuery
                {
                    DaysThreshold = daysThreshold
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

        private void UpdateUI(ExpiringProductsReportData data)
        {
            TotalExpiredText.Text = data.TotalExpired.ToString();
            TotalExpiringText.Text = data.TotalExpiring.ToString();
            TotalValueText.Text = $"L {data.TotalValueAtRisk:N2}";

            var displayItems = data.Products.Select(i => new ExpiringProductDisplayItem
            {
                ProductName = i.ProductName,
                FamilyName = i.FamilyName,
                ExpirationDate = i.ExpirationDate,
                DaysUntilExpiration = i.DaysUntilExpiration,
                CurrentStock = i.CurrentStock,
                TotalValue = i.TotalValue,
                Status = i.Status
            }).ToList();

            DataListView.ItemsSource = displayItems;
        }

        private async void PrintPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_currentData == null) return;

            try
            {
                var pdfBytes = _pdfService.GenerateExpiringProductsReportPdf(_currentData);
                var tempPath = Path.Combine(Path.GetTempPath(), $"ReporteProductosVencer_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
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

    public class ExpiringProductDisplayItem
    {
        public string ProductName { get; set; } = null!;
        public string FamilyName { get; set; } = null!;
        public DateTime? ExpirationDate { get; set; }
        public int DaysUntilExpiration { get; set; }
        public int CurrentStock { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; } = null!;

        public string ExpirationDateFormatted => ExpirationDate?.ToString("dd/MM/yyyy") ?? "-";
        public string TotalValueFormatted => $"L {TotalValue:N2}";

        public Brush StatusBackground => Status switch
        {
            "Vencido" => new SolidColorBrush(Microsoft.UI.Colors.Red),
            "Critico" => new SolidColorBrush(Microsoft.UI.Colors.Orange),
            _ => new SolidColorBrush(Microsoft.UI.Colors.Gold)
        };
    }
}
