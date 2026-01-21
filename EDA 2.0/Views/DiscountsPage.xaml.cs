using EDA.APPLICATION.Features.DiscountFeature.Commands.CreateDiscountCommand;
using EDA.APPLICATION.Features.DiscountFeature.Commands.DeleteDiscountCommand;
using EDA.APPLICATION.Features.DiscountFeature.Commands.UpdateDiscountCommand;
using EDA.APPLICATION.Features.DiscountFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class DiscountsPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public DiscountsPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDiscounts();
        }

        private async Task LoadDiscounts()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllDiscountsQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    DiscountsListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar descuentos");
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
                await LoadDiscounts();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadDiscounts();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadDiscounts();
            }
        }

        private async void NewDiscountButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowDiscountDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Discount discount)
            {
                await ShowDiscountDialog(discount);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Discount discount)
            {
                await ShowDeleteConfirmation(discount);
            }
        }

        private async Task ShowDiscountDialog(Discount? discount)
        {
            bool isEdit = discount != null;

            var nameTextBox = new TextBox
            {
                Header = "Nombre del descuento *",
                PlaceholderText = "Ingrese el nombre del descuento",
                Text = discount?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var percentageNumberBox = new NumberBox
            {
                Header = "Porcentaje *",
                PlaceholderText = "Ingrese el porcentaje",
                Value = discount?.Percentage != null ? (double)discount.Percentage : 0,
                Minimum = 0,
                Maximum = 100,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var content = new StackPanel
            {
                Width = 350,
                Children = { nameTextBox, percentageNumberBox }
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar Descuento" : "Nuevo Descuento",
                Content = content,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var percentage = double.IsNaN(percentageNumberBox.Value) ? 0 : (decimal)percentageNumberBox.Value;
                await SaveDiscount(discount?.Id, nameTextBox.Text?.Trim() ?? string.Empty, percentage, isEdit);
            }
        }

        private async Task SaveDiscount(int? id, string name, decimal percentage, bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdateDiscountCommand
                    {
                        Id = id.Value,
                        Name = name,
                        Percentage = percentage
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Descuento actualizado exitosamente.");
                        await LoadDiscounts();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar descuento");
                    }
                }
                else
                {
                    var command = new CreateDiscountCommand
                    {
                        Name = name,
                        Percentage = percentage
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Descuento creado exitosamente.");
                        await LoadDiscounts();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear descuento");
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

        private async Task ShowDeleteConfirmation(Discount discount)
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
                            Text = $"¿Está seguro que desea eliminar el descuento \"{discount.Name}\"?",
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
                await DeleteDiscount(discount.Id);
            }
        }

        private async Task DeleteDiscount(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeleteDiscountCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Descuento eliminado exitosamente.");
                    await LoadDiscounts();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar descuento");
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
            NewDiscountButton.IsEnabled = !isLoading;
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
