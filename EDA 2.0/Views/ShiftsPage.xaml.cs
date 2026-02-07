using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Features.CompanyFeature.Queries;
using EDA.APPLICATION.Features.ShiftFeature.Commands.CreateShiftCommand;
using EDA.APPLICATION.Features.ShiftFeature.Commands.DeleteShiftCommand;
using EDA.APPLICATION.Features.ShiftFeature.Commands.UpdateShiftCommand;
using EDA.APPLICATION.Features.ShiftFeature.Queries;
using EDA.APPLICATION.Features.UserFeature.Queries;
using EDA.APPLICATION.Interfaces;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class ShiftsPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private List<User> _users = new();

        public ShiftsPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
            await LoadShifts();
        }

        private async Task LoadUsers()
        {
            try
            {
                var result = await _mediator.Send(new GetAllUsersQuery { PageSize = 1000 });
                if (result.Succeeded && result.Data != null)
                {
                    _users = result.Data.Items;
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error al cargar usuarios: {ex.Message}");
            }
        }

        private bool? GetFilterState()
        {
            return FilterComboBox.SelectedIndex switch
            {
                1 => true,   // Abiertos
                2 => false,  // Cerrados
                _ => null    // Todos
            };
        }

        private async Task LoadShifts()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllShiftsQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    IsOpen = GetFilterState(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    ShiftsListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar turnos");
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
                await LoadShifts();
            }
        }

        private async void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            _currentPage = 1;
            await LoadShifts();
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadShifts();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadShifts();
            }
        }

        private async void NewShiftButton_Click(object sender, RoutedEventArgs e)
        {
            var userComboBox = new ComboBox
            {
                Header = "Usuario *",
                PlaceholderText = "Seleccione un usuario",
                ItemsSource = _users,
                DisplayMemberPath = "Name",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var shiftTypeComboBox = new ComboBox
            {
                Header = "Tipo de turno *",
                PlaceholderText = "Seleccione el turno",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };
            shiftTypeComboBox.Items.Add("Matutino");
            shiftTypeComboBox.Items.Add("Vespertino");
            shiftTypeComboBox.Items.Add("Nocturno");

            var initialAmountBox = new NumberBox
            {
                Header = "Monto inicial en caja *",
                PlaceholderText = "0.00",
                Value = 0,
                Minimum = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var content = new StackPanel
            {
                Width = 400,
                Children = { userComboBox, shiftTypeComboBox, initialAmountBox }
            };

            var dialog = new ContentDialog
            {
                Title = "Abrir Turno",
                Content = content,
                PrimaryButtonText = "Abrir",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var selectedUser = userComboBox.SelectedItem as User;
                if (selectedUser == null)
                {
                    await ShowError("Debe seleccionar un usuario.");
                    return;
                }

                if (shiftTypeComboBox.SelectedItem == null)
                {
                    await ShowError("Debe seleccionar un tipo de turno.");
                    return;
                }

                var shiftType = shiftTypeComboBox.SelectedItem.ToString()!;
                decimal initialAmount = double.IsNaN(initialAmountBox.Value) ? 0 : (decimal)initialAmountBox.Value;

                await CreateShift(selectedUser.Id, shiftType, initialAmount);
            }
        }

        private async Task CreateShift(int userId, string shiftType, decimal initialAmount)
        {
            SetLoading(true);

            try
            {
                var command = new CreateShiftCommand
                {
                    UserId = userId,
                    ShiftType = shiftType,
                    InitialAmount = initialAmount
                };

                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Turno abierto exitosamente.");
                    await LoadShifts();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al abrir turno");
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

        private async void CloseShiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Shift shift)
            {
                if (!shift.IsOpen)
                {
                    await ShowError("Este turno ya esta cerrado.");
                    return;
                }

                // Obtener datos de cierre del turno via Query
                var closingResult = await _mediator.Send(new GetShiftClosingDataQuery
                {
                    UserId = shift.UserId,
                    ShiftStartTime = shift.StartTime,
                    InitialAmount = shift.InitialAmount
                });

                if (!closingResult.Succeeded || closingResult.Data == null)
                {
                    await ShowError("Error al obtener datos del turno.");
                    return;
                }

                var closingData = closingResult.Data;

                var infoText = new TextBlock
                {
                    Text = $"Usuario: {shift.User?.Name}\nTurno: {shift.ShiftType}\nInicio: {shift.StartTime:dd/MM/yyyy HH:mm}\nSaldo inicial: L {shift.InitialAmount:N2}\n\nVentas en efectivo: L {closingData.ExpectedCash:N2}\nVentas en tarjeta: L {closingData.ExpectedCard:N2}\nSaldo esperado: L {closingData.ExpectedTotal:N2}",
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var cashBox = new NumberBox
                {
                    Header = "Efectivo en caja *",
                    PlaceholderText = "0.00",
                    Value = double.NaN,
                    Minimum = 0,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var cardBox = new NumberBox
                {
                    Header = "Total en tarjeta *",
                    PlaceholderText = "0.00",
                    Value = double.NaN,
                    Minimum = 0,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
                };

                var content = new StackPanel
                {
                    Width = 400,
                    Children = { infoText, cashBox, cardBox }
                };

                var dialog = new ContentDialog
                {
                    Title = "Cerrar Turno",
                    Content = content,
                    PrimaryButtonText = "Cerrar Turno",
                    CloseButtonText = "Cancelar",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    decimal finalCash = double.IsNaN(cashBox.Value) ? 0 : (decimal)cashBox.Value;
                    decimal finalCard = double.IsNaN(cardBox.Value) ? 0 : (decimal)cardBox.Value;
                    await CloseShift(shift, finalCash, finalCard, closingData.ExpectedCash, closingData.ExpectedCard, closingData.ExpectedTotal, closingData.TotalInvoices, closingData.TotalSales);
                }
            }
        }

        private async Task CloseShift(Shift shift, decimal finalCash, decimal finalCard, decimal expectedCash, decimal expectedCard, decimal expectedTotal, int totalInvoices, decimal totalSales)
        {
            SetLoading(true);

            try
            {
                var command = new UpdateShiftCommand
                {
                    Id = shift.Id,
                    FinalCashAmount = finalCash,
                    FinalCardAmount = finalCard,
                    ExpectedAmount = expectedTotal
                };

                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    decimal finalAmount = finalCash + finalCard + shift.InitialAmount;
                    decimal difference = expectedTotal - finalAmount;

                    var companyResult = await _mediator.Send(new GetCompanyQuery());
                    var company = companyResult.Data;

                    var pdfService = App.Services.GetRequiredService<IShiftReportPdfService>();
                    var reportData = new ShiftReportData
                    {
                        UserName = shift.User?.Name ?? "Usuario",
                        ShiftType = shift.ShiftType,
                        StartTime = shift.StartTime,
                        EndTime = DateTime.Now,
                        InitialAmount = shift.InitialAmount,
                        FinalCashAmount = finalCash,
                        FinalCardAmount = finalCard,
                        FinalAmount = finalAmount,
                        ExpectedCash = expectedCash,
                        ExpectedCard = expectedCard,
                        ExpectedAmount = expectedTotal,
                        Difference = difference,
                        TotalInvoices = totalInvoices,
                        TotalSales = totalSales,
                        CompanyName = company?.Name ?? "Empresa",
                        CompanyAddress = company?.Address1,
                        CompanyRTN = company?.RTN
                    };

                    var pdfBytes = pdfService.GenerateShiftReportPdf(reportData);
                    var tempPath = Path.Combine(Path.GetTempPath(), $"ReporteTurno_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                    await File.WriteAllBytesAsync(tempPath, pdfBytes);
                    Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });

                    await ShowSuccess("Turno cerrado exitosamente.");
                    await LoadShifts();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cerrar turno");
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

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Shift shift)
            {
                if (shift.IsOpen)
                {
                    await ShowError("No se puede eliminar un turno abierto. Cierre el turno primero.");
                    return;
                }

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
                                Text = $"Esta seguro que desea eliminar el turno de \"{shift.User?.Name}\"?",
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
                    await DeleteShift(shift.Id);
                }
            }
        }

        private async Task DeleteShift(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeleteShiftCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Turno eliminado exitosamente.");
                    await LoadShifts();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar turno");
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

        private void ShiftsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Item is Shift shift)
            {
                var container = args.ItemContainer;

                if (shift.IsOpen)
                {
                    container.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(0x15, 0x00, 0x80, 0x00));
                }
                else
                {
                    container.Background = null;
                }

                // Mostrar botón eliminar solo para admin y turnos cerrados
                var deleteBtn = FindChildByName<Button>(container, "DeleteShiftButton");
                if (deleteBtn != null)
                {
                    bool isAdmin = App.CurrentUser?.RoleId == (int)EDA.DOMAIN.Enums.RoleEnum.Admin;
                    deleteBtn.Visibility = (isAdmin && !shift.IsOpen) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private static T? FindChildByName<T>(Microsoft.UI.Xaml.DependencyObject parent, string name) where T : FrameworkElement
        {
            int childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == name)
                    return element;
                var result = FindChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            NewShiftButton.IsEnabled = !isLoading;
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
