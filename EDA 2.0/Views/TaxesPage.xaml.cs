using EDA.APPLICATION.Features.TaxFeature.Commands.CreateTaxCommand;
using EDA.APPLICATION.Features.TaxFeature.Commands.DeleteTaxCommand;
using EDA.APPLICATION.Features.TaxFeature.Commands.UpdateTaxCommand;
using EDA.APPLICATION.Features.TaxFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class TaxesPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public TaxesPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTaxes();
        }

        private async Task LoadTaxes()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllTaxesQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    TaxesListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar impuestos");
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
                await LoadTaxes();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadTaxes();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadTaxes();
            }
        }

        private async void NewTaxButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowTaxDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Tax tax)
            {
                await ShowTaxDialog(tax);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Tax tax)
            {
                await ShowDeleteConfirmation(tax);
            }
        }

        private async Task ShowTaxDialog(Tax? tax)
        {
            bool isEdit = tax != null;

            var nameTextBox = new TextBox
            {
                Header = "Nombre del impuesto *",
                PlaceholderText = "Ingrese el nombre del impuesto",
                Text = tax?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var percentageNumberBox = new NumberBox
            {
                Header = "Porcentaje *",
                PlaceholderText = "Ingrese el porcentaje",
                Value = tax?.Percentage != null ? (double)tax.Percentage : 0,
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
                Title = isEdit ? "Editar Impuesto" : "Nuevo Impuesto",
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
                await SaveTax(tax?.Id, nameTextBox.Text?.Trim() ?? string.Empty, percentage, isEdit);
            }
        }

        private async Task SaveTax(int? id, string name, decimal percentage, bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdateTaxCommand
                    {
                        Id = id.Value,
                        Name = name,
                        Percentage = percentage
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Impuesto actualizado exitosamente.");
                        await LoadTaxes();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar impuesto");
                    }
                }
                else
                {
                    var command = new CreateTaxCommand
                    {
                        Name = name,
                        Percentage = percentage
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Impuesto creado exitosamente.");
                        await LoadTaxes();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear impuesto");
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

        private async Task ShowDeleteConfirmation(Tax tax)
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
                            Text = $"¿Está seguro que desea eliminar el impuesto \"{tax.Name}\"?",
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
                await DeleteTax(tax.Id);
            }
        }

        private async Task DeleteTax(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeleteTaxCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Impuesto eliminado exitosamente.");
                    await LoadTaxes();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar impuesto");
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
            NewTaxButton.IsEnabled = !isLoading;
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
