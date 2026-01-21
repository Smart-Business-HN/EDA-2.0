using EDA.APPLICATION.Features.CustomerFeature.Commands.CreateCustomerCommand;
using EDA.APPLICATION.Features.CustomerFeature.Commands.DeleteCustomerCommand;
using EDA.APPLICATION.Features.CustomerFeature.Commands.UpdateCustomerCommand;
using EDA.APPLICATION.Features.CustomerFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class CustomersPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public CustomersPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomers();
        }

        private async Task LoadCustomers()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllCustomersQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    CustomersListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar clientes");
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
                await LoadCustomers();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadCustomers();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadCustomers();
            }
        }

        private async void NewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowCustomerDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Customer customer)
            {
                await ShowCustomerDialog(customer);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Customer customer)
            {
                await ShowDeleteConfirmation(customer);
            }
        }

        private async Task ShowCustomerDialog(Customer? customer)
        {
            bool isEdit = customer != null;

            var nameTextBox = new TextBox
            {
                Header = "Nombre del cliente *",
                PlaceholderText = "Ingrese el nombre del cliente",
                Text = customer?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var companyTextBox = new TextBox
            {
                Header = "Empresa",
                PlaceholderText = "Ingrese el nombre de la empresa (opcional)",
                Text = customer?.Company ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var emailTextBox = new TextBox
            {
                Header = "Correo electronico",
                PlaceholderText = "Ingrese el correo electronico (opcional)",
                Text = customer?.Email ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var phoneTextBox = new TextBox
            {
                Header = "Numero de telefono",
                PlaceholderText = "Ingrese el numero de telefono (opcional)",
                Text = customer?.PhoneNumber ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var descriptionTextBox = new TextBox
            {
                Header = "Descripcion",
                PlaceholderText = "Ingrese una descripcion (opcional)",
                Text = customer?.Description ?? string.Empty,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 80
            };

            var content = new StackPanel
            {
                Width = 400,
                Children = { nameTextBox, companyTextBox, emailTextBox, phoneTextBox, descriptionTextBox }
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar Cliente" : "Nuevo Cliente",
                Content = content,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await SaveCustomer(
                    customer?.Id,
                    nameTextBox.Text?.Trim() ?? string.Empty,
                    companyTextBox.Text?.Trim(),
                    emailTextBox.Text?.Trim(),
                    phoneTextBox.Text?.Trim(),
                    descriptionTextBox.Text?.Trim(),
                    isEdit);
            }
        }

        private async Task SaveCustomer(int? id, string name, string? company, string? email, string? phoneNumber, string? description, bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdateCustomerCommand
                    {
                        Id = id.Value,
                        Name = name,
                        Company = string.IsNullOrWhiteSpace(company) ? null : company,
                        Email = string.IsNullOrWhiteSpace(email) ? null : email,
                        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                        Description = string.IsNullOrWhiteSpace(description) ? null : description
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Cliente actualizado exitosamente.");
                        await LoadCustomers();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar cliente");
                    }
                }
                else
                {
                    var command = new CreateCustomerCommand
                    {
                        Name = name,
                        Company = string.IsNullOrWhiteSpace(company) ? null : company,
                        Email = string.IsNullOrWhiteSpace(email) ? null : email,
                        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                        Description = string.IsNullOrWhiteSpace(description) ? null : description
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Cliente creado exitosamente.");
                        await LoadCustomers();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear cliente");
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

        private async Task ShowDeleteConfirmation(Customer customer)
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
                            Text = $"Esta seguro que desea eliminar el cliente \"{customer.Name}\"?",
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
                await DeleteCustomer(customer.Id);
            }
        }

        private async Task DeleteCustomer(int id)
        {
            SetLoading(true);

            try
            {
                var command = new DeleteCustomerCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Cliente eliminado exitosamente.");
                    await LoadCustomers();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar cliente");
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
            NewCustomerButton.IsEnabled = !isLoading;
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
