using EDA.DOMAIN.Entities;
using EDA.INFRAESTRUCTURE;
using Microsoft.EntityFrameworkCore;
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
        private readonly DatabaseContext _dbContext;

        public SalesSummaryPage()
        {
            InitializeComponent();
            _dbContext = App.Services.GetRequiredService<DatabaseContext>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Default: mes actual
            var today = DateTime.Today;
            FromDatePicker.Date = new DateTimeOffset(new DateTime(today.Year, today.Month, 1));
            ToDatePicker.Date = new DateTimeOffset(today);

            // Cargar usuarios
            var users = await _dbContext.Users.OrderBy(u => u.Name).ToListAsync();
            UserFilterComboBox.Items.Clear();
            UserFilterComboBox.Items.Add(new User { Id = 0, Name = "Todos" });
            foreach (var u in users)
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

                // Consultar turnos en rango
                var shiftsQuery = _dbContext.Shifts
                    .Include(s => s.User)
                    .Where(s => s.StartTime >= fromDate && s.StartTime < toDate);

                if (filterUserId.HasValue)
                    shiftsQuery = shiftsQuery.Where(s => s.UserId == filterUserId.Value);

                var shifts = await shiftsQuery.OrderByDescending(s => s.StartTime).ToListAsync();

                // Consultar facturas en rango
                var invoicesQuery = _dbContext.Invoices
                    .Where(i => i.Date >= fromDate && i.Date < toDate);

                if (filterUserId.HasValue)
                    invoicesQuery = invoicesQuery.Where(i => i.UserId == filterUserId.Value);

                var invoices = await invoicesQuery.ToListAsync();

                // Detalle por turno
                var shiftDetails = new List<ShiftDetailItem>();
                foreach (var shift in shifts)
                {
                    var endTime = shift.EndTime ?? DateTime.Now;
                    var shiftInvoices = invoices
                        .Where(i => i.UserId == shift.UserId && i.Date >= shift.StartTime && i.Date <= endTime)
                        .ToList();

                    shiftDetails.Add(new ShiftDetailItem
                    {
                        UserName = shift.User?.Name ?? "—",
                        ShiftType = shift.ShiftType,
                        StartTime = shift.StartTime,
                        EndTime = shift.EndTime,
                        InitialAmount = shift.InitialAmount,
                        FinalAmount = shift.FinalAmount,
                        Difference = shift.Difference,
                        InvoiceCount = shiftInvoices.Count,
                        TotalSales = shiftInvoices.Sum(i => i.Total)
                    });
                }

                // Resumen por usuario
                var userSummary = shiftDetails
                    .GroupBy(s => s.UserName)
                    .Select(g => new UserSummaryItem
                    {
                        UserName = g.Key,
                        TotalShifts = g.Count(),
                        TotalInvoices = g.Sum(s => s.InvoiceCount),
                        TotalSales = g.Sum(s => s.TotalSales)
                    })
                    .OrderByDescending(u => u.TotalSales)
                    .ToList();

                // Totales globales
                var grandTotalShifts = shifts.Count;
                var grandTotalInvoices = shiftDetails.Sum(s => s.InvoiceCount);
                var grandTotalSales = shiftDetails.Sum(s => s.TotalSales);

                // Actualizar UI
                TotalShiftsText.Text = grandTotalShifts.ToString();
                TotalInvoicesText.Text = grandTotalInvoices.ToString();
                TotalSalesText.Text = $"L {grandTotalSales:N2}";

                UserSummaryListView.ItemsSource = userSummary;
                ShiftDetailListView.ItemsSource = shiftDetails;
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

    // DTOs internos
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
