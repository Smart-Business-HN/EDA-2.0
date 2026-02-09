using EDA.APPLICATION.Features.CashRegisterFeature.Commands.CreateCashRegisterCommand;
using EDA.APPLICATION.Features.CashRegisterFeature.Commands.DeleteCashRegisterCommand;
using EDA.APPLICATION.Features.CashRegisterFeature.Commands.UpdateCashRegisterCommand;
using EDA.APPLICATION.Features.CashRegisterFeature.Queries;
using EDA.APPLICATION.Features.PrinterConfigurationFeature.Queries;
using EDA.DOMAIN.Entities;
using EDA_2._0.Views.Base;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class CashRegistersPage : CancellablePage
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private List<PrinterConfiguration> _printerConfigurations = new();

        public CashRegistersPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SafeExecuteAsync(async ct =>
            {
                await LoadPrinterConfigurations(ct);
                if (!IsPageActive) return;

                await LoadCashRegisters(ct);
            });
        }

        private async Task LoadPrinterConfigurations(CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetAllPrinterConfigurationsQuery
                {
                    IsActive = true,
                    GetAll = true
                };

                var result = await _mediator.Send(query, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (result.Succeeded && result.Data != null)
                {
                    _printerConfigurations = result.Data.Items.ToList();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (IsPageActive)
                {
                    await ShowError($"Error al cargar impresoras: {ex.Message}");
                }
            }
        }

        private async Task LoadCashRegisters(CancellationToken cancellationToken = default)
        {
            SetLoading(true);

            try
            {
                var query = new GetAllCashRegistersQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (result.Succeeded && result.Data != null)
                {
                    CashRegistersListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar cajas registradoras");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (IsPageActive)
                {
                    await ShowError($"Error: {ex.Message}");
                }
            }
            finally
            {
                if (IsPageActive)
                {
                    SetLoading(false);
                }
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
                await LoadCashRegisters();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadCashRegisters();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadCashRegisters();
            }
        }

        private async void NewButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCashRegisterDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CashRegister cashRegister)
            {
                await ShowCashRegisterDialog(cashRegister);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CashRegister cashRegister)
            {
                await ShowDeleteConfirmation(cashRegister);
            }
        }

        private async Task ShowCashRegisterDialog(CashRegister? cashRegister)
        {
            bool isEdit = cashRegister != null;

            var codeTextBox = new TextBox
            {
                Header = "Codigo *",
                PlaceholderText = "Ej: C001, BAR01",
                Text = cashRegister?.Code ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12),
                MaxLength = 20,
                CharacterCasing = CharacterCasing.Upper
            };

            var nameTextBox = new TextBox
            {
                Header = "Nombre de la caja *",
                PlaceholderText = "Ej: Caja Principal, Caja Bar",
                Text = cashRegister?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var printerCombo = new ComboBox
            {
                Header = "Configuracion de impresora *",
                Margin = new Thickness(0, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                DisplayMemberPath = "Name"
            };

            foreach (var printer in _printerConfigurations)
            {
                printerCombo.Items.Add(printer);
            }

            // Select current printer if editing
            if (cashRegister != null && cashRegister.PrinterConfigurationId > 0)
            {
                var selectedPrinter = _printerConfigurations.FirstOrDefault(p => p.Id == cashRegister.PrinterConfigurationId);
                if (selectedPrinter != null)
                {
                    printerCombo.SelectedItem = selectedPrinter;
                }
            }
            else if (_printerConfigurations.Count > 0)
            {
                printerCombo.SelectedIndex = 0;
            }

            var isActiveToggle = new ToggleSwitch
            {
                Header = "Estado",
                OnContent = "Activo",
                OffContent = "Inactivo",
                IsOn = cashRegister?.IsActive ?? true,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var scrollViewer = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Width = 400,
                    Children =
                    {
                        codeTextBox,
                        nameTextBox,
                        printerCombo,
                        isActiveToggle
                    }
                },
                MaxHeight = 400,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar Caja Registradora" : "Nueva Caja Registradora",
                Content = scrollViewer,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var selectedPrinter = printerCombo.SelectedItem as PrinterConfiguration;

                if (selectedPrinter == null)
                {
                    await ShowError("Debe seleccionar una configuracion de impresora.");
                    return;
                }

                await SaveCashRegister(
                    cashRegister?.Id,
                    codeTextBox.Text?.Trim() ?? string.Empty,
                    nameTextBox.Text?.Trim() ?? string.Empty,
                    selectedPrinter.Id,
                    isActiveToggle.IsOn,
                    isEdit);
            }
        }

        private async Task SaveCashRegister(
            int? id,
            string code,
            string name,
            int printerConfigurationId,
            bool isActive,
            bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdateCashRegisterCommand
                    {
                        Id = id.Value,
                        Code = code,
                        Name = name,
                        PrinterConfigurationId = printerConfigurationId,
                        IsActive = isActive
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Caja registradora actualizada exitosamente.");
                        await LoadCashRegisters();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar caja registradora");
                    }
                }
                else
                {
                    var command = new CreateCashRegisterCommand
                    {
                        Code = code,
                        Name = name,
                        PrinterConfigurationId = printerConfigurationId,
                        IsActive = isActive
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Caja registradora creada exitosamente.");
                        await LoadCashRegisters();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear caja registradora");
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

        private async Task ShowDeleteConfirmation(CashRegister cashRegister)
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
                            Foreground = new SolidColorBrush(Colors.Orange),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = $"Esta seguro que desea eliminar la caja \"{cashRegister.Name}\" ({cashRegister.Code})?",
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
                await DeleteCashRegister(cashRegister.Id);
            }
        }

        private async Task DeleteCashRegister(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeleteCashRegisterCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Caja registradora eliminada exitosamente.");
                    await LoadCashRegisters();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar caja registradora");
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
            NewButton.IsEnabled = !isLoading;
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
                            Foreground = new SolidColorBrush(Colors.Red),
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
                            Foreground = new SolidColorBrush(Colors.Green),
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
