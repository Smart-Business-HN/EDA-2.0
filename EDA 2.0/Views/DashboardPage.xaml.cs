using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.DashboardFeature.Queries;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class DashboardPage : Page
    {
        private readonly IMediator _mediator;

        public DashboardPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDashboardData();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            SetLoading(true);

            try
            {
                var query = new GetDashboardDataQuery();
                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    UpdateUI(result.Data);
                }
                else
                {
                    ShowError(result.Message ?? "Error al cargar datos del dashboard");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void UpdateUI(DashboardData data)
        {
            // Actualizar tarjetas de resumen
            TodaySalesText.Text = $"L {data.TodaySales:N2}";
            TodayInvoicesText.Text = $"{data.TodayInvoices} facturas";

            WeekSalesText.Text = $"L {data.WeekSales:N2}";
            WeekInvoicesText.Text = $"{data.WeekInvoices} facturas";

            MonthSalesText.Text = $"L {data.MonthSales:N2}";
            MonthInvoicesText.Text = $"{data.MonthInvoices} facturas";

            YearSalesText.Text = $"L {data.YearSales:N2}";
            YearInvoicesText.Text = $"{data.YearInvoices} facturas";

            // Gráfico de últimos 7 días
            UpdateLast7DaysChart(data.Last7DaysSales);

            // Gráfico de métodos de pago
            UpdatePaymentMethodsChart(data.PaymentMethods);

            // Gráfico de ventas por hora
            UpdateHourlySalesChart(data.SalesByHour);

            // Gráfico de ventas mensuales
            UpdateMonthlySalesChart(data.Last12MonthsSales);

            // Gráfico de top productos
            UpdateTopProductsChart(data.TopProducts);

            // Gráfico de top familias
            UpdateTopFamiliesChart(data.TopFamilies);

            // Lista de últimas facturas
            UpdateRecentInvoices(data.RecentInvoices);
        }

        private void UpdateLast7DaysChart(List<DailySalesData> data)
        {
            var values = data.Select(d => (double)d.Total).ToArray();
            var labels = data.Select(d => d.DayName).ToArray();

            Last7DaysChart.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    MaxBarWidth = 40
                }
            };

            Last7DaysChart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0
                }
            };

            Last7DaysChart.YAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value => $"L {value:N0}"
                }
            };
        }

        private void UpdatePaymentMethodsChart(List<PaymentMethodData> data)
        {
            if (data == null || data.Count == 0)
            {
                PaymentMethodsChart.Series = Array.Empty<ISeries>();
                return;
            }

            var colors = new SKColor[]
            {
                SKColors.CornflowerBlue,
                SKColors.MediumSeaGreen,
                SKColors.Orange,
                SKColors.MediumPurple,
                SKColors.Coral
            };

            var series = data.Select((pm, index) => new PieSeries<double>
            {
                Values = new double[] { (double)pm.TotalAmount },
                Name = pm.PaymentTypeName,
                Fill = new SolidColorPaint(colors[index % colors.Length]),
                DataLabelsSize = 12,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => pm.PaymentTypeName
            }).ToArray();

            PaymentMethodsChart.Series = series;
        }

        private void UpdateHourlySalesChart(List<HourlySalesData> data)
        {
            if (data == null || data.Count == 0)
            {
                HourlySalesChart.Series = Array.Empty<ISeries>();
                return;
            }

            // Crear array completo de 24 horas
            var fullData = Enumerable.Range(0, 24)
                .Select(h => data.FirstOrDefault(d => d.Hour == h)?.InvoiceCount ?? 0)
                .ToArray();

            var labels = Enumerable.Range(0, 24).Select(h => $"{h:D2}").ToArray();

            HourlySalesChart.Series = new ISeries[]
            {
                new LineSeries<int>
                {
                    Values = fullData,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue.WithAlpha(50)),
                    Stroke = new SolidColorPaint(SKColors.CornflowerBlue, 2),
                    GeometrySize = 6,
                    GeometryFill = new SolidColorPaint(SKColors.CornflowerBlue),
                    GeometryStroke = new SolidColorPaint(SKColors.White, 2)
                }
            };

            HourlySalesChart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0,
                    ForceStepToMin = true,
                    MinStep = 4
                }
            };

            HourlySalesChart.YAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value => $"{value:N0}"
                }
            };
        }

        private void UpdateMonthlySalesChart(List<MonthlySalesData> data)
        {
            if (data == null || data.Count == 0)
            {
                MonthlySalesChart.Series = Array.Empty<ISeries>();
                return;
            }

            var values = data.Select(d => (double)d.Total).ToArray();
            var labels = data.Select(d => d.MonthName).ToArray();

            MonthlySalesChart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.MediumSeaGreen.WithAlpha(50)),
                    Stroke = new SolidColorPaint(SKColors.MediumSeaGreen, 2),
                    GeometrySize = 8,
                    GeometryFill = new SolidColorPaint(SKColors.MediumSeaGreen),
                    GeometryStroke = new SolidColorPaint(SKColors.White, 2)
                }
            };

            MonthlySalesChart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0
                }
            };

            MonthlySalesChart.YAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value => $"L {value/1000:N0}K"
                }
            };
        }

        private void UpdateTopProductsChart(List<TopProductData> data)
        {
            if (data == null || data.Count == 0)
            {
                TopProductsChart.Series = Array.Empty<ISeries>();
                return;
            }

            var values = data.Select(d => d.QuantitySold).ToArray();
            var labels = data.Select(d => TruncateProductName(d.ProductName, 20)).ToArray();

            TopProductsChart.Series = new ISeries[]
            {
                new RowSeries<int>
                {
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.Orange),
                    MaxBarWidth = 25
                }
            };

            TopProductsChart.YAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 0
                }
            };

            TopProductsChart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value => $"{value:N0}"
                }
            };
        }

        private void UpdateTopFamiliesChart(List<TopFamilyData> data)
        {
            if (data == null || data.Count == 0)
            {
                TopFamiliesChart.Series = Array.Empty<ISeries>();
                FamiliesListControl.ItemsSource = null;
                return;
            }

            var colors = new SKColor[]
            {
                SKColors.CornflowerBlue,
                SKColors.MediumSeaGreen,
                SKColors.Orange,
                SKColors.MediumPurple,
                SKColors.Coral
            };

            var series = data.Select((f, index) => new PieSeries<double>
            {
                Values = new double[] { (double)f.TotalRevenue },
                Name = f.FamilyName,
                Fill = new SolidColorPaint(colors[index % colors.Length]),
                DataLabelsSize = 10,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => f.FamilyName
            }).ToArray();

            TopFamiliesChart.Series = series;

            // Actualizar lista de familias
            var familyItems = data.Select(f => new
            {
                f.FamilyName,
                TotalRevenueFormatted = $"L {f.TotalRevenue:N2}"
            }).ToList();

            FamiliesListControl.ItemsSource = familyItems;
        }

        private void UpdateRecentInvoices(List<RecentInvoiceData> data)
        {
            if (data == null || data.Count == 0)
            {
                RecentInvoicesControl.ItemsSource = null;
                return;
            }

            var items = data.Select(i => new
            {
                i.InvoiceNumber,
                DateFormatted = i.Date.ToString("dd/MM/yyyy HH:mm"),
                i.CustomerName,
                TotalFormatted = $"L {i.Total:N2}"
            }).ToList();

            RecentInvoicesControl.ItemsSource = items;
        }

        private static string TruncateProductName(string name, int maxLength)
        {
            if (string.IsNullOrEmpty(name) || name.Length <= maxLength)
                return name ?? string.Empty;
            return name.Substring(0, maxLength - 3) + "...";
        }

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            ContentScrollViewer.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void ShowError(string message)
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
