using EDA.APPLICATION.Features.ExpenseAccountFeature.Commands.CreateExpenseAccountCommand;
using EDA.APPLICATION.Features.ExpenseAccountFeature.Commands.DeleteExpenseAccountCommand;
using EDA.APPLICATION.Features.ExpenseAccountFeature.Commands.UpdateExpenseAccountCommand;
using EDA.APPLICATION.Features.ExpenseAccountFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class ExpenseAccountsPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public ExpenseAccountsPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadExpenseAccounts();
        }

        private async Task LoadExpenseAccounts()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllExpenseAccountsQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    ExpenseAccountsListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar cuentas de gastos");
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
            PageInfoText.Text = $"Página {_currentPage} de {_totalPages}";
        }

        private async void SearchTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                _currentPage = 1;
                await LoadExpenseAccounts();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadExpenseAccounts();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadExpenseAccounts();
            }
        }

        private async void NewExpenseAccountButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowExpenseAccountDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ExpenseAccount expenseAccount)
            {
                await ShowExpenseAccountDialog(expenseAccount);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ExpenseAccount expenseAccount)
            {
                await ShowDeleteConfirmation(expenseAccount);
            }
        }

        private async Task ShowExpenseAccountDialog(ExpenseAccount? expenseAccount)
        {
            bool isEdit = expenseAccount != null;

            var nameTextBox = new TextBox
            {
                Header = "Nombre de la cuenta *",
                PlaceholderText = "Ingrese el nombre de la cuenta",
                Text = expenseAccount?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var content = new StackPanel
            {
                Width = 350,
                Children = { nameTextBox }
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar Cuenta de Gastos" : "Nueva Cuenta de Gastos",
                Content = content,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await SaveExpenseAccount(expenseAccount?.Id, nameTextBox.Text?.Trim() ?? string.Empty, isEdit);
            }
        }

        private async Task SaveExpenseAccount(int? id, string name, bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdateExpenseAccountCommand
                    {
                        Id = id.Value,
                        Name = name
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Cuenta de gastos actualizada exitosamente.");
                        await LoadExpenseAccounts();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar cuenta de gastos");
                    }
                }
                else
                {
                    var command = new CreateExpenseAccountCommand
                    {
                        Name = name
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Cuenta de gastos creada exitosamente.");
                        await LoadExpenseAccounts();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear cuenta de gastos");
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

        private async Task ShowDeleteConfirmation(ExpenseAccount expenseAccount)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirmar eliminacion",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uE897",
                            FontSize = 48,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = $"¿Está seguro que desea eliminar la cuenta \"{expenseAccount.Name}\"?",
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                },
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await DeleteExpenseAccount(expenseAccount.Id);
            }
        }

        private async Task DeleteExpenseAccount(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeleteExpenseAccountCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Cuenta de gastos eliminada exitosamente.");
                    await LoadExpenseAccounts();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar cuenta de gastos");
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

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            NewExpenseAccountButton.IsEnabled = !isLoading;
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
    }
}
