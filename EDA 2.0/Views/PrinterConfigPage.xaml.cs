using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.PrinterConfigurationFeature.Commands.CreatePrinterConfigurationCommand;
using EDA.APPLICATION.Features.PrinterConfigurationFeature.Commands.DeletePrinterConfigurationCommand;
using EDA.APPLICATION.Features.PrinterConfigurationFeature.Commands.UpdatePrinterConfigurationCommand;
using EDA.APPLICATION.Features.PrinterConfigurationFeature.Queries;
using EDA.APPLICATION.Interfaces;
using EDA.DOMAIN.Entities;
using EDA_2._0.Views.Base;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class PrinterConfigPage : CancellablePage
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public PrinterConfigPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SafeExecuteAsync(LoadPrinters);
        }

        private async Task LoadPrinters(CancellationToken cancellationToken = default)
        {
            SetLoading(true);

            try
            {
                var query = new GetAllPrinterConfigurationsQuery
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
                    PrintersListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar configuraciones de impresora");
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
                await LoadPrinters();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadPrinters();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadPrinters();
            }
        }

        private async void NewButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowPrinterDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PrinterConfiguration config)
            {
                await ShowPrinterDialog(config);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PrinterConfiguration config)
            {
                await ShowDeleteConfirmation(config);
            }
        }

        private async Task ShowPrinterDialog(PrinterConfiguration? config)
        {
            bool isEdit = config != null;

            var nameTextBox = new TextBox
            {
                Header = "Nombre de la configuracion *",
                PlaceholderText = "Ej: Impresora Termica Caja 1",
                Text = config?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var printerTypeCombo = new ComboBox
            {
                Header = "Tipo de impresora *",
                Margin = new Thickness(0, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            printerTypeCombo.Items.Add(new ComboBoxItem { Content = "Termica", Tag = 1 });
            printerTypeCombo.Items.Add(new ComboBoxItem { Content = "Matricial", Tag = 2 });
            printerTypeCombo.SelectedIndex = config != null ? config.PrinterType - 1 : 0;

            var printerNameTextBox = new TextBox
            {
                Header = "Nombre de impresora del sistema (opcional)",
                PlaceholderText = "Ej: EPSON TM-T20",
                Text = config?.PrinterName ?? string.Empty,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var searchPrinterButton = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Children =
                    {
                        new FontIcon { Glyph = "\uE721", FontSize = 14 },
                        new TextBlock { Text = "Buscar" }
                    }
                },
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(8, 0, 0, 0)
            };

            searchPrinterButton.Click += (s, args) =>
            {
                ShowPrinterSearchFlyout(searchPrinterButton, printerNameTextBox);
            };

            var printerNamePanel = new Grid
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            printerNamePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            printerNamePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(printerNameTextBox, 0);
            Grid.SetColumn(searchPrinterButton, 1);
            printerNamePanel.Children.Add(printerNameTextBox);
            printerNamePanel.Children.Add(searchPrinterButton);

            var copyStrategyCombo = new ComboBox
            {
                Header = "Estrategia de copias *",
                Margin = new Thickness(0, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            copyStrategyCombo.Items.Add(new ComboBoxItem { Content = "Copia Carbon (Matricial)", Tag = 1 });
            copyStrategyCombo.Items.Add(new ComboBoxItem { Content = "Doble Impresion", Tag = 2 });
            copyStrategyCombo.Items.Add(new ComboBoxItem { Content = "Fin del Dia", Tag = 3 });
            copyStrategyCombo.Items.Add(new ComboBoxItem { Content = "Solo Digital", Tag = 4 });
            copyStrategyCombo.SelectedIndex = config != null ? config.CopyStrategy - 1 : 1;

            var fontSizeBox = new NumberBox
            {
                Header = "Tamano de fuente (pt)",
                Value = config?.FontSize ?? 8,
                Minimum = 6,
                Maximum = 16,
                Margin = new Thickness(0, 0, 0, 12),
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var copiesCountBox = new NumberBox
            {
                Header = "Numero de copias",
                Value = config?.CopiesCount ?? 1,
                Minimum = 1,
                Maximum = 5,
                Margin = new Thickness(0, 0, 0, 12),
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
            };

            var printWidthCombo = new ComboBox
            {
                Header = "Ancho de papel *",
                Margin = new Thickness(0, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            printWidthCombo.Items.Add(new ComboBoxItem { Content = "58mm", Tag = 58 });
            printWidthCombo.Items.Add(new ComboBoxItem { Content = "80mm", Tag = 80 });
            printWidthCombo.Items.Add(new ComboBoxItem { Content = "Carta (216mm)", Tag = 216 });

            // Set selected based on current value
            int widthIndex = 1; // Default 80mm
            if (config != null)
            {
                widthIndex = config.PrintWidth switch
                {
                    58 => 0,
                    80 => 1,
                    216 => 2,
                    _ => 1
                };
            }
            printWidthCombo.SelectedIndex = widthIndex;

            var isActiveToggle = new ToggleSwitch
            {
                Header = "Estado",
                OnContent = "Activo",
                OffContent = "Inactivo",
                IsOn = config?.IsActive ?? true,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var scrollViewer = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Width = 400,
                    Children =
                    {
                        nameTextBox,
                        printerTypeCombo,
                        printerNamePanel,
                        copyStrategyCombo,
                        fontSizeBox,
                        copiesCountBox,
                        printWidthCombo,
                        isActiveToggle
                    }
                },
                MaxHeight = 500,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar Configuracion" : "Nueva Configuracion",
                Content = scrollViewer,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var printerType = (int)((ComboBoxItem)printerTypeCombo.SelectedItem).Tag;
                var copyStrategy = (int)((ComboBoxItem)copyStrategyCombo.SelectedItem).Tag;
                var printWidth = (int)((ComboBoxItem)printWidthCombo.SelectedItem).Tag;

                await SavePrinter(
                    config?.Id,
                    nameTextBox.Text?.Trim() ?? string.Empty,
                    printerType,
                    printerNameTextBox.Text?.Trim(),
                    (int)fontSizeBox.Value,
                    copyStrategy,
                    (int)copiesCountBox.Value,
                    printWidth,
                    isActiveToggle.IsOn,
                    isEdit);
            }
        }

        private void ShowPrinterSearchFlyout(Button targetButton, TextBox targetTextBox)
        {
            var printerService = App.Services.GetService<IPrinterDiscoveryService>();
            if (printerService == null)
            {
                // Show inline error instead of dialog
                targetTextBox.Text = "Error: Servicio no disponible";
                return;
            }

            // Create loading indicator
            var loadingPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new ProgressRing { IsActive = true, Width = 32, Height = 32 },
                    new TextBlock
                    {
                        Text = "Buscando impresoras...",
                        Margin = new Thickness(0, 8, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        FontSize = 12
                    }
                }
            };

            var printersList = new ListView
            {
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 300,
                Visibility = Visibility.Collapsed
            };

            var noResultsText = new TextBlock
            {
                Text = "No se encontraron impresoras",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Gray),
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 12, 0, 12)
            };

            var contentPanel = new StackPanel
            {
                Width = 400,
                MinHeight = 100
            };
            contentPanel.Children.Add(loadingPanel);
            contentPanel.Children.Add(printersList);
            contentPanel.Children.Add(noResultsText);

            var flyout = new Flyout
            {
                Content = contentPanel,
                Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom
            };

            // Handle selection
            printersList.DoubleTapped += (s, args) =>
            {
                if (printersList.SelectedItem is ListViewItem item && item.Tag is DiscoveredPrinter printer)
                {
                    targetTextBox.Text = printer.Name;
                    flyout.Hide();
                }
            };

            // Add select button at the bottom
            var selectButton = new Button
            {
                Content = "Seleccionar",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 8, 0, 0),
                Visibility = Visibility.Collapsed
            };
            selectButton.Click += (s, args) =>
            {
                if (printersList.SelectedItem is ListViewItem item && item.Tag is DiscoveredPrinter printer)
                {
                    targetTextBox.Text = printer.Name;
                    flyout.Hide();
                }
            };
            contentPanel.Children.Add(selectButton);

            printersList.SelectionChanged += (s, args) =>
            {
                selectButton.IsEnabled = printersList.SelectedItem != null;
            };

            // Show flyout first
            flyout.ShowAt(targetButton);

            // Start discovery in background
            _ = Task.Run(async () =>
            {
                try
                {
                    var printers = await printerService.DiscoverPrintersAsync();

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        // Sort: Default first, then by connection type (non-virtual first), then alphabetically
                        var sortedPrinters = printers
                            .OrderByDescending(p => p.IsDefault)
                            .ThenBy(p => p.ConnectionType == PrinterConnectionType.Virtual)
                            .ThenBy(p => p.Name)
                            .ToList();

                        // Create visual items for the list
                        printersList.Items.Clear();
                        foreach (var printer in sortedPrinters)
                        {
                            var item = new ListViewItem
                            {
                                Tag = printer,
                                Content = CreatePrinterListItemCompact(printer)
                            };
                            printersList.Items.Add(item);
                        }

                        loadingPanel.Visibility = Visibility.Collapsed;

                        if (sortedPrinters.Count > 0)
                        {
                            printersList.Visibility = Visibility.Visible;
                            selectButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            noResultsText.Visibility = Visibility.Visible;
                        }
                    });
                }
                catch (Exception ex)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        loadingPanel.Visibility = Visibility.Collapsed;
                        noResultsText.Text = $"Error: {ex.Message}";
                        noResultsText.Visibility = Visibility.Visible;
                    });
                }
            });
        }

        private StackPanel CreatePrinterListItemCompact(DiscoveredPrinter printer)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Padding = new Thickness(4, 8, 4, 8)
            };

            // Printer icon
            var printerIcon = new FontIcon
            {
                Glyph = "\uE749",
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Name
            var nameText = new TextBlock
            {
                Text = printer.Name,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 250,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            panel.Children.Add(printerIcon);
            panel.Children.Add(nameText);

            // Status indicator
            if (printer.IsOnline)
            {
                var onlineIndicator = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Colors.Green),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4, 0, 0, 0)
                };
                panel.Children.Add(onlineIndicator);
            }

            // Default indicator
            if (printer.IsDefault)
            {
                var defaultText = new TextBlock
                {
                    Text = "*",
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.DodgerBlue),
                    VerticalAlignment = VerticalAlignment.Center
                };
                panel.Children.Add(defaultText);
            }

            return panel;
        }

        private async Task SavePrinter(
            int? id,
            string name,
            int printerType,
            string? printerName,
            int fontSize,
            int copyStrategy,
            int copiesCount,
            int printWidth,
            bool isActive,
            bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdatePrinterConfigurationCommand
                    {
                        Id = id.Value,
                        Name = name,
                        PrinterType = printerType,
                        PrinterName = string.IsNullOrWhiteSpace(printerName) ? null : printerName,
                        FontSize = fontSize,
                        CopyStrategy = copyStrategy,
                        CopiesCount = copiesCount,
                        PrintWidth = printWidth,
                        IsActive = isActive
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Configuracion actualizada exitosamente.");
                        await LoadPrinters();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar configuracion");
                    }
                }
                else
                {
                    var command = new CreatePrinterConfigurationCommand
                    {
                        Name = name,
                        PrinterType = printerType,
                        PrinterName = string.IsNullOrWhiteSpace(printerName) ? null : printerName,
                        FontSize = fontSize,
                        CopyStrategy = copyStrategy,
                        CopiesCount = copiesCount,
                        PrintWidth = printWidth,
                        IsActive = isActive
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Configuracion creada exitosamente.");
                        await LoadPrinters();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear configuracion");
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

        private async Task ShowDeleteConfirmation(PrinterConfiguration config)
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
                            Text = $"Esta seguro que desea eliminar la configuracion \"{config.Name}\"?",
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
                await DeletePrinter(config.Id);
            }
        }

        private async Task DeletePrinter(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeletePrinterConfigurationCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Configuracion eliminada exitosamente.");
                    await LoadPrinters();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar configuracion");
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

    // Converters
    public class PrinterTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int printerType)
            {
                return printerType switch
                {
                    1 => "Termica",
                    2 => "Matricial",
                    _ => "Desconocido"
                };
            }
            return "Desconocido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class CopyStrategyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int strategy)
            {
                return strategy switch
                {
                    1 => "Copia Carbon",
                    2 => "Doble Impresion",
                    3 => "Fin del Dia",
                    4 => "Solo Digital",
                    _ => "Desconocido"
                };
            }
            return "Desconocido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class PrintWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int width)
            {
                return $"{width}mm";
            }
            return "80mm";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isActive)
            {
                return isActive
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 252, 231))
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 226, 226));
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isActive)
            {
                return isActive ? "Activo" : "Inactivo";
            }
            return "Desconocido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class OnlineStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isOnline)
            {
                return isOnline
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 252, 231))  // Green
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 226, 226)); // Red
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class OnlineStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isOnline)
                return isOnline ? "En linea" : "Fuera de linea";
            return "Desconocido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
