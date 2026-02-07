using EDA.APPLICATION.Features.InvoiceFeature.Queries.GetAllInvoicesQuery;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class InvoicesPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public InvoicesPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInvoices();
        }

        private async Task LoadInvoices()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllInvoicesQuery
                {
                    SearchTerm = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? null : SearchTextBox.Text.Trim(),
                    FromDate = FromDatePicker.SelectedDate?.DateTime,
                    ToDate = ToDatePicker.SelectedDate?.DateTime,
                    CustomerRtn = string.IsNullOrWhiteSpace(CustomerRtnTextBox.Text) ? null : CustomerRtnTextBox.Text.Trim(),
                    CustomerName = string.IsNullOrWhiteSpace(CustomerNameTextBox.Text) ? null : CustomerNameTextBox.Text.Trim(),
                    InvoiceNumber = string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text) ? null : InvoiceNumberTextBox.Text.Trim(),
                    UserName = string.IsNullOrWhiteSpace(UserNameTextBox.Text) ? null : UserNameTextBox.Text.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    InvoicesListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar facturas");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void UpdatePaginationUI(bool hasPrevious, bool hasNext)
        {
            PreviousPageButton.IsEnabled = hasPrevious;
            NextPageButton.IsEnabled = hasNext;
            PageInfoText.Text = $"Pagina {_currentPage} de {_totalPages}";
        }

        private async void SearchTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                _currentPage = 1;
                await LoadInvoices();
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            await LoadInvoices();
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
            CustomerRtnTextBox.Text = string.Empty;
            CustomerNameTextBox.Text = string.Empty;
            InvoiceNumberTextBox.Text = string.Empty;
            UserNameTextBox.Text = string.Empty;

            _currentPage = 1;
            await LoadInvoices();
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadInvoices();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadInvoices();
            }
        }

        private void CreateInvoice_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.MainWindow as MainWindow;
            mainWindow?.NavigateToInvoiceCreate();
        }

        private void ViewDetailButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Invoice invoice)
            {
                // Navigate to detail page
                var mainWindow = App.MainWindow as MainWindow;
                mainWindow?.NavigateToInvoiceDetail(invoice.Id);
            }
        }

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            SearchButton.IsEnabled = !isLoading;
        }

        private async Task ShowError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uEA39",
                            FontSize = 48,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                },
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
