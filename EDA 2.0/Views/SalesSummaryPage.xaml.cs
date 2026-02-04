using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.SalesSummaryFeature.Queries;
using EDA.APPLICATION.Features.UserFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class SalesSummaryPage : Page
    {
        private readonly IMediator _mediator;
        private List<User> _users = new();

        public SalesSummaryPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Default: mes actual
            var today = DateTime.Today;
            FromDatePicker.Date = new DateTimeOffset(new DateTime(today.Year, today.Month, 1));
            ToDatePicker.Date = new DateTimeOffset(today);

            // Cargar usuarios via Query
            var usersResult = await _mediator.Send(new GetAllUsersQuery { PageSize = 1000 });
            if (usersResult.Succeeded && usersResult.Data != null)
            {
                _users = usersResult.Data.Items;
            }

            UserFilterComboBox.Items.Clear();
            UserFilterComboBox.Items.Add(new User { Id = 0, Name = "Todos" });
            foreach (var u in _users)
                UserFilterComboBox.Items.Add(u);
            UserFilterComboBox.SelectedIndex = 0;

            await LoadData();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            LoadingRing.IsActive = true;

            try
            {
                var fromDate = FromDatePicker.Date.DateTime.Date;
                var toDate = ToDatePicker.Date.DateTime.Date.AddDays(1); // inclusive

                int? filterUserId = null;
                if (UserFilterComboBox.SelectedItem is User selectedUser && selectedUser.Id > 0)
                    filterUserId = selectedUser.Id;

                // Usar GetSalesSummaryQuery
                var result = await _mediator.Send(new GetSalesSummaryQuery
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    UserId = filterUserId
                });

                if (result.Succeeded && result.Data != null)
                {
                    var data = result.Data;

                    // Actualizar UI
                    TotalShiftsText.Text = data.GrandTotalShifts.ToString();
                    TotalInvoicesText.Text = data.GrandTotalInvoices.ToString();
                    TotalSalesText.Text = $"L {data.GrandTotalSales:N2}";

                    // Convertir a DTOs locales para formateo
                    var userSummary = data.UserSummaries.Select(u => new UserSummaryItem
                    {
                        UserName = u.UserName,
                        TotalShifts = u.TotalShifts,
                        TotalInvoices = u.TotalInvoices,
                        TotalSales = u.TotalSales
                    }).ToList();

                    var shiftDetails = data.ShiftDetails.Select(s => new ShiftDetailItem
                    {
                        UserName = s.UserName,
                        ShiftType = s.ShiftType,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        InitialAmount = s.InitialAmount,
                        FinalAmount = s.FinalAmount,
                        Difference = s.Difference,
                        InvoiceCount = s.InvoiceCount,
                        TotalSales = s.TotalSales
                    }).ToList();

                    UserSummaryListView.ItemsSource = userSummary;
                    ShiftDetailListView.ItemsSource = shiftDetails;
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = result.Message ?? "Error al cargar datos",
                        CloseButtonText = "Aceptar",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
        }
    }

    // DTOs internos para formateo en UI
    public class UserSummaryItem
    {
        public string UserName { get; set; } = null!;
        public int TotalShifts { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalSales { get; set; }
        public string TotalSalesFormatted => $"L {TotalSales:N2}";
    }

    public class ShiftDetailItem
    {
        public string UserName { get; set; } = null!;
        public string ShiftType { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public decimal? Difference { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalSales { get; set; }

        public string StartTimeFormatted => StartTime.ToString("dd/MM/yyyy HH:mm");
        public string EndTimeFormatted => EndTime?.ToString("dd/MM/yyyy HH:mm") ?? "Abierto";
        public string InitialAmountFormatted => $"L {InitialAmount:N2}";
        public string FinalAmountFormatted => FinalAmount.HasValue ? $"L {FinalAmount:N2}" : "—";
        public string DifferenceFormatted => Difference.HasValue ? $"L {Difference:N2}" : "—";
        public string TotalSalesFormatted => $"L {TotalSales:N2}";
    }
}
