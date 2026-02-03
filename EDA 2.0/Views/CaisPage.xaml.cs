using EDA.APPLICATION.Features.CaiFeature.Commands.CreateCaiCommand;
using EDA.APPLICATION.Features.CaiFeature.Commands.DeleteCaiCommand;
using EDA.APPLICATION.Features.CaiFeature.Commands.UpdateCaiCommand;
using EDA.APPLICATION.Features.CaiFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class CaisPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        // Regex para validaciones
        private static readonly Regex CodeRegex = new Regex(
            @"^[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{6}-[0-9A-Fa-f]{2}$",
            RegexOptions.Compiled);

        private static readonly Regex PrefixRegex = new Regex(
            @"^\d{3}-\d{3}-\d{2}-$",
            RegexOptions.Compiled);

        public CaisPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCais();
        }

        private async Task LoadCais()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllCaisQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    CaisListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar CAIs");
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
                await LoadCais();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadCais();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadCais();
            }
        }

        private async void NewCaiButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCaiDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Cai cai)
            {
                await ShowCaiDialog(cai);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Cai cai)
            {
                await ShowDeleteConfirmation(cai);
            }
        }

        private async Task ShowCaiDialog(Cai? cai)
        {
            bool isEdit = cai != null;

            var nameTextBox = new TextBox
            {
                Header = "Nombre del CAI *",
                PlaceholderText = "Ingrese un nombre descriptivo",
                Text = cai?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var codeTextBox = new TextBox
            {
                Header = "Codigo CAI * (Formato: XXXXXX-XXXXXX-XXXXXX-XXXXXX-XXXXXX-XX)",
                PlaceholderText = "Ejemplo: 489D01-9282C4-A74690-B28A5F-07F128-D5",
                Text = cai?.Code ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12),
                CharacterCasing = CharacterCasing.Upper,
                MaxLength = 37
            };

            // Agregar evento para formatear el código automáticamente
            codeTextBox.TextChanged += (s, args) =>
            {
                FormatCodeInput(codeTextBox);
            };

            var prefixTextBox = new TextBox
            {
                Header = "Prefijo * (Formato: XXX-XXX-XX-)",
                PlaceholderText = "Ejemplo: 000-002-01-",
                Text = cai?.Prefix ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12),
                MaxLength = 12
            };

            // Agregar evento para formatear el prefijo automáticamente
            prefixTextBox.TextChanged += (s, args) =>
            {
                FormatPrefixInput(prefixTextBox);
            };

            var fromDatePicker = new DatePicker
            {
                Header = "Fecha de inicio *",
                Date = cai?.FromDate != null ? new DateTimeOffset(cai.FromDate) : DateTimeOffset.Now,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var toDatePicker = new DatePicker
            {
                Header = "Fecha de fin *",
                Date = cai?.ToDate != null ? new DateTimeOffset(cai.ToDate) : DateTimeOffset.Now.AddYears(1),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var initialCorrelativeBox = new NumberBox
            {
                Header = "Correlativo inicial *",
                PlaceholderText = "Ejemplo: 1",
                Value = cai?.InitialCorrelative ?? 1,
                Minimum = 1,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var finalCorrelativeBox = new NumberBox
            {
                Header = "Correlativo final *",
                PlaceholderText = "Ejemplo: 500",
                Value = cai?.FinalCorrelative ?? 500,
                Minimum = 1,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var isActiveToggle = new ToggleSwitch
            {
                Header = "CAI Activo",
                IsOn = cai?.IsActive ?? true,
                OnContent = "Activo",
                OffContent = "Inactivo",
                Margin = new Thickness(0, 0, 0, 12)
            };

            // Campo editable para correlativo actual (solo en edición)
            NumberBox? currentCorrelativeBox = null;
            if (isEdit && cai != null)
            {
                currentCorrelativeBox = new NumberBox
                {
                    Header = "Correlativo actual (número de siguiente factura)",
                    PlaceholderText = "Ejemplo: 73",
                    Value = cai.CurrentCorrelative,
                    Minimum = 1,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                    Margin = new Thickness(0, 0, 0, 12)
                };
            }

            var content = new StackPanel
            {
                Width = 450
            };

            content.Children.Add(nameTextBox);
            content.Children.Add(codeTextBox);
            content.Children.Add(prefixTextBox);
            content.Children.Add(fromDatePicker);
            content.Children.Add(toDatePicker);
            content.Children.Add(initialCorrelativeBox);
            content.Children.Add(finalCorrelativeBox);

            if (currentCorrelativeBox != null)
            {
                content.Children.Add(currentCorrelativeBox);
            }

            content.Children.Add(isActiveToggle);

            var scrollViewer = new ScrollViewer
            {
                Content = content,
                MaxHeight = 500,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar CAI" : "Nuevo CAI",
                Content = scrollViewer,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Validaciones del lado del cliente
                var code = codeTextBox.Text?.Trim().ToUpperInvariant() ?? string.Empty;
                var prefix = prefixTextBox.Text?.Trim() ?? string.Empty;

                if (!CodeRegex.IsMatch(code))
                {
                    await ShowError("El codigo del CAI debe tener el formato: XXXXXX-XXXXXX-XXXXXX-XXXXXX-XXXXXX-XX");
                    return;
                }

                if (!PrefixRegex.IsMatch(prefix))
                {
                    await ShowError("El prefijo debe tener el formato: XXX-XXX-XX- (ejemplo: 000-002-01-)");
                    return;
                }

                var fromDate = fromDatePicker.Date.DateTime;
                var toDate = toDatePicker.Date.DateTime;

                if (toDate <= fromDate)
                {
                    await ShowError("La fecha de fin debe ser posterior a la fecha de inicio.");
                    return;
                }

                var initialCorrelative = double.IsNaN(initialCorrelativeBox.Value) ? 1 : (int)initialCorrelativeBox.Value;
                var finalCorrelative = double.IsNaN(finalCorrelativeBox.Value) ? 500 : (int)finalCorrelativeBox.Value;

                if (finalCorrelative <= initialCorrelative)
                {
                    await ShowError("El correlativo final debe ser mayor al correlativo inicial.");
                    return;
                }

                int? currentCorrelative = null;
                if (currentCorrelativeBox != null && !double.IsNaN(currentCorrelativeBox.Value))
                {
                    currentCorrelative = (int)currentCorrelativeBox.Value;
                }

                await SaveCai(
                    cai?.Id,
                    nameTextBox.Text?.Trim() ?? string.Empty,
                    code,
                    fromDate,
                    toDate,
                    initialCorrelative,
                    finalCorrelative,
                    prefix,
                    isActiveToggle.IsOn,
                    currentCorrelative,
                    isEdit);
            }
        }

        private void FormatCodeInput(TextBox textBox)
        {
            var text = textBox.Text?.ToUpperInvariant() ?? string.Empty;
            var cursorPosition = textBox.SelectionStart;

            // Remover caracteres no válidos (solo hex y guiones)
            var cleanText = Regex.Replace(text, @"[^0-9A-F-]", "");

            // Remover guiones para reconstruir
            var digitsOnly = cleanText.Replace("-", "");

            // Limitar a 32 caracteres hex
            if (digitsOnly.Length > 32)
            {
                digitsOnly = digitsOnly.Substring(0, 32);
            }

            // Reconstruir con guiones en posiciones correctas: 6-6-6-6-6-2
            var formatted = "";
            for (int i = 0; i < digitsOnly.Length; i++)
            {
                if (i == 6 || i == 12 || i == 18 || i == 24 || i == 30)
                {
                    formatted += "-";
                }
                formatted += digitsOnly[i];
            }

            if (formatted != text)
            {
                textBox.Text = formatted;
                textBox.SelectionStart = Math.Min(formatted.Length, cursorPosition + (formatted.Length - text.Length));
            }
        }

        private void FormatPrefixInput(TextBox textBox)
        {
            var text = textBox.Text ?? string.Empty;
            var cursorPosition = textBox.SelectionStart;

            // Remover caracteres no válidos (solo dígitos y guiones)
            var cleanText = Regex.Replace(text, @"[^0-9-]", "");

            // Remover guiones para reconstruir
            var digitsOnly = cleanText.Replace("-", "");

            // Limitar a 8 dígitos
            if (digitsOnly.Length > 8)
            {
                digitsOnly = digitsOnly.Substring(0, 8);
            }

            // Reconstruir con guiones en posiciones correctas: 3-3-2-
            var formatted = "";
            for (int i = 0; i < digitsOnly.Length; i++)
            {
                if (i == 3 || i == 6 || i == 8)
                {
                    formatted += "-";
                }
                formatted += digitsOnly[i];
            }

            // Agregar guion final si tiene 8 dígitos
            if (digitsOnly.Length == 8 && !formatted.EndsWith("-"))
            {
                formatted += "-";
            }

            if (formatted != text)
            {
                textBox.Text = formatted;
                textBox.SelectionStart = Math.Min(formatted.Length, cursorPosition + (formatted.Length - text.Length));
            }
        }

        private async Task SaveCai(int? id, string name, string code, DateTime fromDate, DateTime toDate,
            int initialCorrelative, int finalCorrelative, string prefix, bool isActive, int? currentCorrelative, bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdateCaiCommand
                    {
                        Id = id.Value,
                        Name = name,
                        Code = code,
                        FromDate = fromDate,
                        ToDate = toDate,
                        InitialCorrelative = initialCorrelative,
                        FinalCorrelative = finalCorrelative,
                        Prefix = prefix,
                        IsActive = isActive,
                        CurrentCorrelative = currentCorrelative
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("CAI actualizado exitosamente.");
                        await LoadCais();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar CAI");
                    }
                }
                else
                {
                    var command = new CreateCaiCommand
                    {
                        Name = name,
                        Code = code,
                        FromDate = fromDate,
                        ToDate = toDate,
                        InitialCorrelative = initialCorrelative,
                        FinalCorrelative = finalCorrelative,
                        Prefix = prefix,
                        IsActive = isActive
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("CAI creado exitosamente.");
                        await LoadCais();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear CAI");
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

        private async Task ShowDeleteConfirmation(Cai cai)
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
                            Text = $"Esta seguro que desea eliminar el CAI \"{cai.Name}\"?",
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = "Nota: Solo se puede eliminar si no tiene facturas emitidas.",
                            FontSize = 12,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
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
                await DeleteCai(cai.Id);
            }
        }

        private async Task DeleteCai(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeleteCaiCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("CAI eliminado exitosamente.");
                    await LoadCais();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar CAI");
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
            NewCaiButton.IsEnabled = !isLoading;
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
