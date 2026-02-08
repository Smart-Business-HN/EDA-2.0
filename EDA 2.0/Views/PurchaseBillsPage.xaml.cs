using EDA.APPLICATION.Features.ExpenseAccountFeature.Queries;
using EDA.APPLICATION.Features.ProviderFeature.Queries;
using EDA.APPLICATION.Features.PurchaseBillFeature.Commands.CreatePurchaseBillCommand;
using EDA.APPLICATION.Features.PurchaseBillFeature.Commands.UpdatePurchaseBillCommand;
using EDA.APPLICATION.Features.PurchaseBillFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class PurchaseBillsPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private List<Provider> _providers = new();
        private List<ExpenseAccount> _expenseAccounts = new();
        private int? _editPurchaseBillId = null;

        public PurchaseBillsPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        public void SetEditPurchaseBillId(int purchaseBillId)
        {
            _editPurchaseBillId = purchaseBillId;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadFiltersData();
            await LoadPurchaseBills();

            // Si hay un ID de edicion, cargar el dialogo de edicion
            if (_editPurchaseBillId.HasValue)
            {
                var editId = _editPurchaseBillId.Value;
                _editPurchaseBillId = null; // Limpiar para evitar re-abrir

                var result = await _mediator.Send(new GetPurchaseBillByIdQuery { Id = editId });
                if (result.Succeeded && result.Data != null)
                {
                    await ShowPurchaseBillDialog(result.Data);
                }
            }
        }

        private async Task LoadFiltersData()
        {
            try
            {
                // Cargar proveedores
                var providersResult = await _mediator.Send(new GetAllProvidersQuery { GetAll = true });
                if (providersResult.Succeeded && providersResult.Data != null)
                {
                    _providers = providersResult.Data.Items;
                    ProviderComboBox.ItemsSource = _providers;
                }

                // Cargar cuentas de gastos
                var expenseAccountsResult = await _mediator.Send(new GetAllExpenseAccountsQuery { GetAll = true });
                if (expenseAccountsResult.Succeeded && expenseAccountsResult.Data != null)
                {
                    _expenseAccounts = expenseAccountsResult.Data.Items;
                    ExpenseAccountComboBox.ItemsSource = _expenseAccounts;
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error al cargar filtros: {ex.Message}");
            }
        }

        private async Task LoadPurchaseBills()
        {
            SetLoading(true);

            try
            {
                int? statusId = null;
                if (StatusComboBox.SelectedItem is ComboBoxItem statusItem && !string.IsNullOrEmpty(statusItem.Tag?.ToString()))
                {
                    statusId = int.Parse(statusItem.Tag.ToString()!);
                }

                var query = new GetAllPurchaseBillsQuery
                {
                    SearchTerm = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? null : SearchTextBox.Text.Trim(),
                    ProviderId = (ProviderComboBox.SelectedItem as Provider)?.Id,
                    ExpenseAccountId = (ExpenseAccountComboBox.SelectedItem as ExpenseAccount)?.Id,
                    StatusId = statusId,
                    FromDate = FromDatePicker.SelectedDate?.DateTime,
                    ToDate = ToDatePicker.SelectedDate?.DateTime,
                    PageNumber = _currentPage,
                    PageSize = _pageSize
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    PurchaseBillsListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar facturas de compra");
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
                await LoadPurchaseBills();
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            await LoadPurchaseBills();
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
            ProviderComboBox.SelectedItem = null;
            ExpenseAccountComboBox.SelectedItem = null;
            StatusComboBox.SelectedIndex = 0;

            _currentPage = 1;
            await LoadPurchaseBills();
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadPurchaseBills();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadPurchaseBills();
            }
        }

        private async void NewPurchaseBillButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowPurchaseBillDialog(null);
        }

        private void ViewDetailButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PurchaseBill purchaseBill)
            {
                var mainWindow = App.MainWindow as MainWindow;
                mainWindow?.NavigateToPurchaseBillDetail(purchaseBill.Id);
            }
        }

        private async Task ShowPurchaseBillDialog(PurchaseBill? purchaseBill)
        {
            bool isEdit = purchaseBill != null;

            // Provider ComboBox
            var providerComboBox = new ComboBox
            {
                Header = "Proveedor *",
                PlaceholderText = "Seleccione un proveedor",
                ItemsSource = _providers,
                DisplayMemberPath = "Name",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };
            if (isEdit && purchaseBill!.ProviderId > 0)
            {
                providerComboBox.SelectedItem = _providers.Find(p => p.Id == purchaseBill.ProviderId);
            }

            // ExpenseAccount ComboBox
            var expenseAccountComboBox = new ComboBox
            {
                Header = "Cuenta de Gastos *",
                PlaceholderText = "Seleccione una cuenta",
                ItemsSource = _expenseAccounts,
                DisplayMemberPath = "Name",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };
            if (isEdit && purchaseBill!.ExpenseAccountId > 0)
            {
                expenseAccountComboBox.SelectedItem = _expenseAccounts.Find(ea => ea.Id == purchaseBill.ExpenseAccountId);
            }

            var invoiceNumberTextBox = new TextBox
            {
                Header = "Numero de Factura *",
                PlaceholderText = "Ej: 001-001-01-00000001",
                Text = purchaseBill?.InvoiceNumber ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var invoiceDatePicker = new DatePicker
            {
                Header = "Fecha de Factura *",
                Margin = new Thickness(0, 0, 0, 12)
            };
            if (isEdit)
            {
                invoiceDatePicker.SelectedDate = purchaseBill!.InvoiceDate;
            }

            var caiTextBox = new TextBox
            {
                Header = "CAI * (XXXXXX-XXXXXX-XXXXXX-XXXXXX-XXXXXX-XX)",
                PlaceholderText = "A1B2C3-D4E5F6-789012-ABCDEF-123456-78",
                Text = purchaseBill?.Cai ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12),
                MaxLength = 37 // 32 hex chars + 5 dashes
            };
            caiTextBox.TextChanged += CaiTextBox_TextChanged;

            // Campos de impuestos
            var exemptNumberBox = new NumberBox
            {
                Header = "Exento",
                PlaceholderText = "0.00",
                Value = isEdit ? (double)purchaseBill!.Exempt : 0,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var exoneratedNumberBox = new NumberBox
            {
                Header = "Exonerado",
                PlaceholderText = "0.00",
                Value = isEdit ? (double)purchaseBill!.Exonerated : 0,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var taxed15NumberBox = new NumberBox
            {
                Header = "Gravado 15%",
                PlaceholderText = "0.00",
                Value = isEdit ? (double)purchaseBill!.TaxedAt15Percent : 0,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var taxes15NumberBox = new NumberBox
            {
                Header = "ISV 15%",
                PlaceholderText = "0.00",
                Value = isEdit ? (double)purchaseBill!.Taxes15Percent : 0,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var taxed18NumberBox = new NumberBox
            {
                Header = "Gravado 18%",
                PlaceholderText = "0.00",
                Value = isEdit ? (double)purchaseBill!.TaxedAt18Percent : 0,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var taxes18NumberBox = new NumberBox
            {
                Header = "ISV 18%",
                PlaceholderText = "0.00",
                Value = isEdit ? (double)purchaseBill!.Taxes18Percent : 0,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var totalNumberBox = new NumberBox
            {
                Header = "Total *",
                PlaceholderText = "0.00",
                Value = isEdit ? (double)purchaseBill!.Total : 0,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var creditDaysNumberBox = new NumberBox
            {
                Header = "Dias de Credito",
                PlaceholderText = "0",
                Value = isEdit && purchaseBill!.CreditDays.HasValue ? purchaseBill.CreditDays.Value : double.NaN,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var dueDatePicker = new DatePicker
            {
                Header = "Fecha de Vencimiento",
                Margin = new Thickness(0, 0, 0, 12)
            };
            if (isEdit && purchaseBill!.DueDate.HasValue)
            {
                dueDatePicker.SelectedDate = purchaseBill.DueDate;
            }

            var scrollViewer = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Width = 400,
                    Children =
                    {
                        providerComboBox,
                        expenseAccountComboBox,
                        invoiceNumberTextBox,
                        invoiceDatePicker,
                        caiTextBox,
                        exemptNumberBox,
                        exoneratedNumberBox,
                        taxed15NumberBox,
                        taxes15NumberBox,
                        taxed18NumberBox,
                        taxes18NumberBox,
                        totalNumberBox,
                        creditDaysNumberBox,
                        dueDatePicker
                    }
                },
                MaxHeight = 500,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar Factura de Compra" : "Nueva Factura de Compra",
                Content = scrollViewer,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var provider = providerComboBox.SelectedItem as Provider;
                var expenseAccount = expenseAccountComboBox.SelectedItem as ExpenseAccount;

                if (provider == null)
                {
                    await ShowError("Debe seleccionar un proveedor.");
                    return;
                }

                if (expenseAccount == null)
                {
                    await ShowError("Debe seleccionar una cuenta de gastos.");
                    return;
                }

                if (!invoiceDatePicker.SelectedDate.HasValue)
                {
                    await ShowError("Debe seleccionar la fecha de factura.");
                    return;
                }

                await SavePurchaseBill(
                    purchaseBill?.Id,
                    provider.Id,
                    expenseAccount.Id,
                    invoiceNumberTextBox.Text?.Trim() ?? string.Empty,
                    invoiceDatePicker.SelectedDate.Value.DateTime,
                    caiTextBox.Text?.Trim() ?? string.Empty,
                    double.IsNaN(exemptNumberBox.Value) ? 0 : (decimal)exemptNumberBox.Value,
                    double.IsNaN(exoneratedNumberBox.Value) ? 0 : (decimal)exoneratedNumberBox.Value,
                    double.IsNaN(taxed15NumberBox.Value) ? 0 : (decimal)taxed15NumberBox.Value,
                    double.IsNaN(taxes15NumberBox.Value) ? 0 : (decimal)taxes15NumberBox.Value,
                    double.IsNaN(taxed18NumberBox.Value) ? 0 : (decimal)taxed18NumberBox.Value,
                    double.IsNaN(taxes18NumberBox.Value) ? 0 : (decimal)taxes18NumberBox.Value,
                    double.IsNaN(totalNumberBox.Value) ? 0 : (decimal)totalNumberBox.Value,
                    double.IsNaN(creditDaysNumberBox.Value) ? null : (int?)creditDaysNumberBox.Value,
                    dueDatePicker.SelectedDate?.DateTime,
                    isEdit);
            }
        }

        private async Task SavePurchaseBill(
            int? id,
            int providerId,
            int expenseAccountId,
            string invoiceNumber,
            DateTime invoiceDate,
            string cai,
            decimal exempt,
            decimal exonerated,
            decimal taxedAt15Percent,
            decimal taxes15Percent,
            decimal taxedAt18Percent,
            decimal taxes18Percent,
            decimal total,
            int? creditDays,
            DateTime? dueDate,
            bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdatePurchaseBillCommand
                    {
                        Id = id.Value,
                        ProviderId = providerId,
                        ExpenseAccountId = expenseAccountId,
                        InvoiceNumber = invoiceNumber,
                        InvoiceDate = invoiceDate,
                        Cai = cai,
                        Exempt = exempt,
                        Exonerated = exonerated,
                        TaxedAt15Percent = taxedAt15Percent,
                        Taxes15Percent = taxes15Percent,
                        TaxedAt18Percent = taxedAt18Percent,
                        Taxes18Percent = taxes18Percent,
                        Total = total,
                        CreditDays = creditDays,
                        DueDate = dueDate
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Factura de compra actualizada exitosamente.");
                        await LoadPurchaseBills();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar factura de compra");
                    }
                }
                else
                {
                    var command = new CreatePurchaseBillCommand
                    {
                        ProviderId = providerId,
                        ExpenseAccountId = expenseAccountId,
                        InvoiceNumber = invoiceNumber,
                        InvoiceDate = invoiceDate,
                        Cai = cai,
                        Exempt = exempt,
                        Exonerated = exonerated,
                        TaxedAt15Percent = taxedAt15Percent,
                        Taxes15Percent = taxes15Percent,
                        TaxedAt18Percent = taxedAt18Percent,
                        Taxes18Percent = taxes18Percent,
                        Total = total,
                        CreditDays = creditDays,
                        DueDate = dueDate
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Factura de compra creada exitosamente.");
                        await LoadPurchaseBills();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear factura de compra");
                    }
                }
            }
            catch (EDA.APPLICATION.Exceptions.ValidationException vex)
            {
                await ShowError(string.Join("\n", vex.Errors));
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

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            SearchButton.IsEnabled = !isLoading;
            NewPurchaseBillButton.IsEnabled = !isLoading;
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

        private async Task ShowSuccess(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Exito",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uE73E",
                            FontSize = 48,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
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

        private void CaiTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            // Solo permitir caracteres hexadecimales (0-9, A-F, a-f)
            var hex = new string(textBox.Text.Where(c =>
                char.IsDigit(c) ||
                (c >= 'A' && c <= 'F') ||
                (c >= 'a' && c <= 'f')).ToArray()).ToUpper();

            // Limitar a 32 caracteres hex
            if (hex.Length > 32)
                hex = hex.Substring(0, 32);

            // Formatear: 6-6-6-6-6-2
            var formatted = FormatCai(hex);

            if (textBox.Text != formatted)
            {
                textBox.Text = formatted;
                textBox.SelectionStart = formatted.Length;
            }
        }

        private static string FormatCai(string hex)
        {
            var sb = new StringBuilder();
            int[] groupSizes = { 6, 6, 6, 6, 6, 2 };
            int pos = 0;

            for (int i = 0; i < groupSizes.Length && pos < hex.Length; i++)
            {
                if (i > 0) sb.Append('-');
                int take = Math.Min(groupSizes[i], hex.Length - pos);
                sb.Append(hex.Substring(pos, take));
                pos += take;
            }

            return sb.ToString();
        }
    }
}
