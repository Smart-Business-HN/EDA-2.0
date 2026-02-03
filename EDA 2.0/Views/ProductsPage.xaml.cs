using EDA.APPLICATION.Features.FamilyFeature.Commands.CreateFamilyCommand;
using EDA.APPLICATION.Features.FamilyFeature.Queries;
using EDA.APPLICATION.Features.ProductFeature.Commands.CreateProductCommand;
using EDA.APPLICATION.Features.ProductFeature.Commands.DeleteProductCommand;
using EDA.APPLICATION.Features.ProductFeature.Commands.UpdateProductCommand;
using EDA.APPLICATION.Features.ProductFeature.Queries;
using EDA.APPLICATION.Features.TaxFeature.Queries;
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
    public sealed partial class ProductsPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private List<Family> _families = new();
        private List<Tax> _taxes = new();

        public ProductsPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadFamiliesAndTaxes();
            await LoadProducts();
        }

        private async Task LoadFamiliesAndTaxes()
        {
            try
            {
                // Cargar familias
                var familiesResult = await _mediator.Send(new GetAllFamiliesQuery { GetAll = true });
                if (familiesResult.Succeeded && familiesResult.Data != null)
                {
                    _families = familiesResult.Data.Items.ToList();
                }

                // Cargar impuestos
                var taxesResult = await _mediator.Send(new GetAllTaxesQuery { GetAll = true });
                if (taxesResult.Succeeded && taxesResult.Data != null)
                {
                    _taxes = taxesResult.Data.Items.ToList();
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error al cargar datos: {ex.Message}");
            }
        }

        private async Task LoadProducts()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllProductsQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    ProductsListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar productos");
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
                await LoadProducts();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadProducts();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadProducts();
            }
        }

        private async void NewProductButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowProductDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                await ShowProductDialog(product);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                await ShowDeleteConfirmation(product);
            }
        }

        private async Task ShowProductDialog(Product? product)
        {
            bool isEdit = product != null;
            bool shouldSave = false;
            Family? selectedFamilyResult = null;
            Tax? selectedTaxResult = null;
            string nameResult = string.Empty;
            string? barcodeResult = null;
            decimal priceResult = 0;
            int stockResult = 0;
            int minStockResult = 0;
            int maxStockResult = 0;
            DateTime? expirationDateResult = null;
            DateTime? dateResult = null;

            // Loop para manejar el caso de crear familia y volver al diálogo
            while (true)
            {
                // Recargar familias antes de mostrar el diálogo
                await LoadFamiliesAndTaxes();

                var nameTextBox = new TextBox
                {
                    Header = "Nombre del producto *",
                    PlaceholderText = "Ingrese el nombre del producto",
                    Text = shouldSave ? nameResult : (product?.Name ?? string.Empty),
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var barcodeTextBox = new TextBox
                {
                    Header = "Codigo de barras",
                    PlaceholderText = "Ingrese el codigo de barras (opcional)",
                    Text = shouldSave ? (barcodeResult ?? string.Empty) : (product?.Barcode ?? string.Empty),
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var priceNumberBox = new NumberBox
                {
                    Header = "Precio *",
                    PlaceholderText = "Ingrese el precio",
                    Value = shouldSave ? (double)priceResult : (product?.Price != null ? (double)product.Price : 0),
                    Minimum = 0,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var stockNumberBox = new NumberBox
                {
                    Header = "Stock *",
                    PlaceholderText = "Ingrese el stock",
                    Value = shouldSave ? stockResult : (product?.Stock ?? 0),
                    Minimum = 0,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var minStockNumberBox = new NumberBox
                {
                    Header = "Stock Mínimo",
                    PlaceholderText = "0 = sin mínimo",
                    Value = shouldSave ? minStockResult : (product?.MinStock ?? 0),
                    Minimum = 0,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var maxStockNumberBox = new NumberBox
                {
                    Header = "Stock Máximo",
                    PlaceholderText = "0 = sin máximo",
                    Value = shouldSave ? maxStockResult : (product?.MaxStock ?? 0),
                    Minimum = 0,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var expirationDatePicker = new DatePicker
                {
                    Header = "Fecha de Vencimiento",
                    Date = shouldSave && expirationDateResult.HasValue
                        ? new DateTimeOffset(expirationDateResult.Value)
                        : (product?.ExpirationDate != null ? new DateTimeOffset(product.ExpirationDate.Value) : DateTimeOffset.Now),
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var expirationCheckBox = new CheckBox
                {
                    Content = "Tiene fecha de vencimiento",
                    IsChecked = shouldSave ? expirationDateResult.HasValue : product?.ExpirationDate != null,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                expirationDatePicker.Visibility = (expirationCheckBox.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
                expirationCheckBox.Checked += (s, a) => expirationDatePicker.Visibility = Visibility.Visible;
                expirationCheckBox.Unchecked += (s, a) => expirationDatePicker.Visibility = Visibility.Collapsed;

                // ComboBox de Familia con botón de crear
                var familyComboBox = new ComboBox
                {
                    Header = "Familia *",
                    PlaceholderText = "Seleccione una familia",
                    ItemsSource = _families,
                    DisplayMemberPath = "Name",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 8, 0)
                };

                // Seleccionar familia
                if (selectedFamilyResult != null)
                {
                    familyComboBox.SelectedItem = _families.FirstOrDefault(f => f.Id == selectedFamilyResult.Id);
                }
                else if (product != null && product.FamilyId > 0)
                {
                    familyComboBox.SelectedItem = _families.FirstOrDefault(f => f.Id == product.FamilyId);
                }

                var newFamilyButton = new Button
                {
                    Content = "+",
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                // Flag para indicar si se debe crear una nueva familia
                bool createNewFamily = false;
                ContentDialog? mainDialog = null;

                newFamilyButton.Click += (s, args) =>
                {
                    createNewFamily = true;
                    mainDialog?.Hide();
                };

                var familyPanel = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 12)
                };
                familyPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                familyPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                Grid.SetColumn(familyComboBox, 0);
                Grid.SetColumn(newFamilyButton, 1);
                familyPanel.Children.Add(familyComboBox);
                familyPanel.Children.Add(newFamilyButton);

                // ComboBox de Impuesto
                var taxComboBox = new ComboBox
                {
                    Header = "Impuesto *",
                    PlaceholderText = "Seleccione un impuesto",
                    ItemsSource = _taxes,
                    DisplayMemberPath = "Name",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 12)
                };

                // Seleccionar impuesto
                if (selectedTaxResult != null)
                {
                    taxComboBox.SelectedItem = _taxes.FirstOrDefault(t => t.Id == selectedTaxResult.Id);
                }
                else if (product != null && product.TaxId > 0)
                {
                    taxComboBox.SelectedItem = _taxes.FirstOrDefault(t => t.Id == product.TaxId);
                }

                var datePicker = new DatePicker
                {
                    Header = "Fecha",
                    Date = shouldSave && dateResult.HasValue
                        ? new DateTimeOffset(dateResult.Value)
                        : (product?.Date != null ? new DateTimeOffset(product.Date.Value) : DateTimeOffset.Now),
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var content = new StackPanel
                {
                    Width = 400,
                    Children = { nameTextBox, barcodeTextBox, priceNumberBox, stockNumberBox, minStockNumberBox, maxStockNumberBox, expirationCheckBox, expirationDatePicker, familyPanel, taxComboBox, datePicker }
                };

                mainDialog = new ContentDialog
                {
                    Title = isEdit ? "Editar Producto" : "Nuevo Producto",
                    Content = content,
                    PrimaryButtonText = "Guardar",
                    CloseButtonText = "Cancelar",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                var result = await mainDialog.ShowAsync();

                // Guardar valores actuales antes de procesar
                nameResult = nameTextBox.Text?.Trim() ?? string.Empty;
                barcodeResult = string.IsNullOrWhiteSpace(barcodeTextBox.Text) ? null : barcodeTextBox.Text.Trim();
                priceResult = double.IsNaN(priceNumberBox.Value) ? 0 : (decimal)priceNumberBox.Value;
                stockResult = double.IsNaN(stockNumberBox.Value) ? 0 : (int)stockNumberBox.Value;
                minStockResult = double.IsNaN(minStockNumberBox.Value) ? 0 : (int)minStockNumberBox.Value;
                maxStockResult = double.IsNaN(maxStockNumberBox.Value) ? 0 : (int)maxStockNumberBox.Value;
                expirationDateResult = expirationCheckBox.IsChecked == true ? expirationDatePicker.Date.DateTime : null;
                dateResult = datePicker.Date.DateTime;
                selectedFamilyResult = familyComboBox.SelectedItem as Family;
                selectedTaxResult = taxComboBox.SelectedItem as Tax;

                // Si se presionó el botón de crear familia
                if (createNewFamily)
                {
                    var newFamily = await ShowCreateFamilyDialog();
                    if (newFamily != null)
                    {
                        selectedFamilyResult = newFamily;
                    }
                    shouldSave = true; // Mantener los valores ingresados
                    continue; // Volver a mostrar el diálogo
                }

                // Si se presionó Guardar
                if (result == ContentDialogResult.Primary)
                {
                    if (selectedFamilyResult == null)
                    {
                        await ShowError("Debe seleccionar una familia.");
                        return;
                    }

                    if (selectedTaxResult == null)
                    {
                        await ShowError("Debe seleccionar un impuesto.");
                        return;
                    }

                    await SaveProduct(
                        product?.Id,
                        nameResult,
                        barcodeResult,
                        dateResult,
                        priceResult,
                        stockResult,
                        minStockResult,
                        maxStockResult,
                        expirationDateResult,
                        selectedFamilyResult.Id,
                        selectedTaxResult.Id,
                        isEdit);
                }

                // Salir del loop (ya sea por Guardar o Cancelar)
                break;
            }
        }

        private async Task<Family?> ShowCreateFamilyDialog()
        {
            var nameTextBox = new TextBox
            {
                Header = "Nombre de la familia *",
                PlaceholderText = "Ingrese el nombre de la familia"
            };

            var content = new StackPanel
            {
                Width = 300,
                Children = { nameTextBox }
            };

            var dialog = new ContentDialog
            {
                Title = "Nueva Familia",
                Content = content,
                PrimaryButtonText = "Crear",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var name = nameTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    await ShowError("El nombre de la familia no puede estar vacío.");
                    return null;
                }

                try
                {
                    var command = new CreateFamilyCommand { Name = name };
                    var createResult = await _mediator.Send(command);

                    if (createResult.Succeeded && createResult.Data != null)
                    {
                        return createResult.Data;
                    }
                    else
                    {
                        await ShowError(createResult.Message ?? "Error al crear la familia.");
                        return null;
                    }
                }
                catch (EDA.APPLICATION.Exceptions.ValidationException vex)
                {
                    await ShowError(string.Join("\n", vex.Errors));
                    return null;
                }
                catch (Exception ex)
                {
                    await ShowError($"Error: {ex.Message}");
                    return null;
                }
            }

            return null;
        }

        private async Task SaveProduct(int? id, string name, string? barcode, DateTime? date, decimal price, int stock, int minStock, int maxStock, DateTime? expirationDate, int familyId, int taxId, bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdateProductCommand
                    {
                        Id = id.Value,
                        Name = name,
                        Barcode = barcode,
                        Date = date,
                        Price = price,
                        Stock = stock,
                        MinStock = minStock,
                        MaxStock = maxStock,
                        ExpirationDate = expirationDate,
                        FamilyId = familyId,
                        TaxId = taxId
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Producto actualizado exitosamente.");
                        await LoadProducts();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar producto");
                    }
                }
                else
                {
                    var command = new CreateProductCommand
                    {
                        Name = name,
                        Barcode = barcode,
                        Date = date,
                        Price = price,
                        Stock = stock,
                        MinStock = minStock,
                        MaxStock = maxStock,
                        ExpirationDate = expirationDate,
                        FamilyId = familyId,
                        TaxId = taxId
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Producto creado exitosamente.");
                        await LoadProducts();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear producto");
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

        private async Task ShowDeleteConfirmation(Product product)
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
                            Text = $"Esta seguro que desea eliminar el producto \"{product.Name}\"?",
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
                await DeleteProduct(product.Id);
            }
        }

        private async Task DeleteProduct(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeleteProductCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Producto eliminado exitosamente.");
                    await LoadProducts();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar producto");
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

        private void ProductsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Item is Product product)
            {
                if (product.Stock == 0)
                {
                    // Stock agotado - rojo más intenso
                    args.ItemContainer.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(0x40, 0xFF, 0x00, 0x00));
                }
                else if (product.MinStock > 0 && product.Stock <= product.MinStock)
                {
                    // Stock bajo - rojo suave
                    args.ItemContainer.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(0x25, 0xFF, 0x00, 0x00));
                }
                else
                {
                    args.ItemContainer.Background = null;
                }
            }
        }

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            NewProductButton.IsEnabled = !isLoading;
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
