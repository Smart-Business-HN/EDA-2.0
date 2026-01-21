using EDA.APPLICATION.Features.PaymentTypeFeature.Commands.CreatePaymentTypeCommand;
using EDA.APPLICATION.Features.PaymentTypeFeature.Commands.DeletePaymentTypeCommand;
using EDA.APPLICATION.Features.PaymentTypeFeature.Commands.UpdatePaymentTypeCommand;
using EDA.APPLICATION.Features.PaymentTypeFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class PaymentTypesPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public PaymentTypesPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPaymentTypes();
        }

        private async Task LoadPaymentTypes()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllPaymentTypesQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    PaymentTypesListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar tipos de pago");
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
                await LoadPaymentTypes();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadPaymentTypes();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadPaymentTypes();
            }
        }

        private async void NewPaymentTypeButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowPaymentTypeDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PaymentType paymentType)
            {
                // Verificar si es el tipo de pago protegido (Id = 1)
                if (paymentType.Id == 1)
                {
                    await ShowError("El tipo de pago 'Efectivo' no puede ser modificado.");
                    return;
                }
                await ShowPaymentTypeDialog(paymentType);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PaymentType paymentType)
            {
                // Verificar si es el tipo de pago protegido (Id = 1)
                if (paymentType.Id == 1)
                {
                    await ShowError("El tipo de pago 'Efectivo' no puede ser eliminado.");
                    return;
                }
                await ShowDeleteConfirmation(paymentType);
            }
        }

        private async Task ShowPaymentTypeDialog(PaymentType? paymentType)
        {
            bool isEdit = paymentType != null;

            var nameTextBox = new TextBox
            {
                Header = "Nombre del tipo de pago *",
                PlaceholderText = "Ingrese el nombre del tipo de pago",
                Text = paymentType?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var content = new StackPanel
            {
                Width = 350,
                Children = { nameTextBox }
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar Tipo de Pago" : "Nuevo Tipo de Pago",
                Content = content,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await SavePaymentType(paymentType?.Id, nameTextBox.Text?.Trim() ?? string.Empty, isEdit);
            }
        }

        private async Task SavePaymentType(int? id, string name, bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdatePaymentTypeCommand
                    {
                        Id = id.Value,
                        Name = name
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Tipo de pago actualizado exitosamente.");
                        await LoadPaymentTypes();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar tipo de pago");
                    }
                }
                else
                {
                    var command = new CreatePaymentTypeCommand
                    {
                        Name = name
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Tipo de pago creado exitosamente.");
                        await LoadPaymentTypes();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear tipo de pago");
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

        private async Task ShowDeleteConfirmation(PaymentType paymentType)
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
                            Text = $"¿Está seguro que desea eliminar el tipo de pago \"{paymentType.Name}\"?",
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
                await DeletePaymentType(paymentType.Id);
            }
        }

        private async Task DeletePaymentType(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeletePaymentTypeCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Tipo de pago eliminado exitosamente.");
                    await LoadPaymentTypes();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar tipo de pago");
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
            NewPaymentTypeButton.IsEnabled = !isLoading;
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
