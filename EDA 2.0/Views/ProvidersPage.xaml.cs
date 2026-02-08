using EDA.APPLICATION.Features.ProviderFeature.Commands.CreateProviderCommand;
using EDA.APPLICATION.Features.ProviderFeature.Commands.DeleteProviderCommand;
using EDA.APPLICATION.Features.ProviderFeature.Commands.UpdateProviderCommand;
using EDA.APPLICATION.Features.ProviderFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class ProvidersPage : Page
    {
        private readonly IMediator _mediator;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public ProvidersPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProviders();
        }

        private async Task LoadProviders()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllProvidersQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize,
                    GetAll = false
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    ProvidersListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar proveedores");
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
                await LoadProviders();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadProviders();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadProviders();
            }
        }

        private async void NewProviderButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowProviderDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Provider provider)
            {
                await ShowProviderDialog(provider);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Provider provider)
            {
                await ShowDeleteConfirmation(provider);
            }
        }

        private async Task ShowProviderDialog(Provider? provider)
        {
            bool isEdit = provider != null;

            var nameTextBox = new TextBox
            {
                Header = "Nombre del proveedor *",
                PlaceholderText = "Ingrese el nombre del proveedor",
                Text = provider?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var rtnTextBox = new TextBox
            {
                Header = "RTN * (14 digitos)",
                PlaceholderText = "01019021333211",
                Text = provider?.RTN ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12),
                MaxLength = 14
            };
            rtnTextBox.TextChanged += RtnTextBox_TextChanged;

            var phoneTextBox = new TextBox
            {
                Header = "Telefono (####-####)",
                PlaceholderText = "8818-7765",
                Text = provider?.PhoneNumber ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12),
                MaxLength = 9
            };
            phoneTextBox.TextChanged += PhoneTextBox_TextChanged;

            var emailTextBox = new TextBox
            {
                Header = "Correo electronico",
                PlaceholderText = "Ingrese el correo electronico (opcional)",
                Text = provider?.Email ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var contactPersonTextBox = new TextBox
            {
                Header = "Persona de contacto",
                PlaceholderText = "Ingrese el nombre de la persona de contacto (opcional)",
                Text = provider?.ContactPerson ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var contactPhoneTextBox = new TextBox
            {
                Header = "Telefono de contacto (####-####)",
                PlaceholderText = "8818-7765",
                Text = provider?.ContactPhoneNumber ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12),
                MaxLength = 9
            };
            contactPhoneTextBox.TextChanged += PhoneTextBox_TextChanged;

            var contactEmailTextBox = new TextBox
            {
                Header = "Correo del contacto",
                PlaceholderText = "Ingrese el correo del contacto (opcional)",
                Text = provider?.ContactEmail ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var addressTextBox = new TextBox
            {
                Header = "Direccion",
                PlaceholderText = "Ingrese la direccion (opcional)",
                Text = provider?.Address ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12),
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 60
            };

            var websiteTextBox = new TextBox
            {
                Header = "Sitio web",
                PlaceholderText = "Ingrese la URL del sitio web (opcional)",
                Text = provider?.WebsiteUrl ?? string.Empty
            };

            var scrollViewer = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Width = 400,
                    Children =
                    {
                        nameTextBox,
                        rtnTextBox,
                        phoneTextBox,
                        emailTextBox,
                        contactPersonTextBox,
                        contactPhoneTextBox,
                        contactEmailTextBox,
                        addressTextBox,
                        websiteTextBox
                    }
                },
                MaxHeight = 450,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar Proveedor" : "Nuevo Proveedor",
                Content = scrollViewer,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await SaveProvider(
                    provider?.Id,
                    nameTextBox.Text?.Trim() ?? string.Empty,
                    rtnTextBox.Text?.Trim() ?? string.Empty,
                    phoneTextBox.Text?.Trim() ?? string.Empty,
                    emailTextBox.Text?.Trim() ?? string.Empty,
                    contactPersonTextBox.Text?.Trim(),
                    contactPhoneTextBox.Text?.Trim(),
                    contactEmailTextBox.Text?.Trim(),
                    addressTextBox.Text?.Trim(),
                    websiteTextBox.Text?.Trim(),
                    isEdit);
            }
        }

        private async Task SaveProvider(
            int? id,
            string name,
            string rtn,
            string phoneNumber,
            string email,
            string? contactPerson,
            string? contactPhoneNumber,
            string? contactEmail,
            string? address,
            string? websiteUrl,
            bool isEdit)
        {
            SetLoading(true);

            try
            {
                var currentUser = App.CurrentUser?.Name ?? "Sistema";

                if (isEdit && id.HasValue)
                {
                    var command = new UpdateProviderCommand
                    {
                        Id = id.Value,
                        Name = name,
                        RTN = rtn,
                        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                        Email = string.IsNullOrWhiteSpace(email) ? null : email,
                        ContactPerson = string.IsNullOrWhiteSpace(contactPerson) ? null : contactPerson,
                        ContactPhoneNumber = string.IsNullOrWhiteSpace(contactPhoneNumber) ? null : contactPhoneNumber,
                        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail,
                        Address = string.IsNullOrWhiteSpace(address) ? null : address,
                        WebsiteUrl = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl,
                        ModificatedBy = currentUser
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Proveedor actualizado exitosamente.");
                        await LoadProviders();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar proveedor");
                    }
                }
                else
                {
                    var command = new CreateProviderCommand
                    {
                        Name = name,
                        RTN = rtn,
                        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                        Email = string.IsNullOrWhiteSpace(email) ? null : email,
                        ContactPerson = string.IsNullOrWhiteSpace(contactPerson) ? null : contactPerson,
                        ContactPhoneNumber = string.IsNullOrWhiteSpace(contactPhoneNumber) ? null : contactPhoneNumber,
                        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail,
                        Address = string.IsNullOrWhiteSpace(address) ? null : address,
                        WebsiteUrl = string.IsNullOrWhiteSpace(websiteUrl) ? null : websiteUrl,
                        CreatedBy = currentUser
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Proveedor creado exitosamente.");
                        await LoadProviders();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear proveedor");
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

        private async Task ShowDeleteConfirmation(Provider provider)
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
                            Text = $"Esta seguro que desea eliminar el proveedor \"{provider.Name}\"?",
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
                await DeleteProvider(provider.Id);
            }
        }

        private async Task DeleteProvider(int id)
        {
            SetLoading(true);

            try
            {
                var currentUser = App.CurrentUser?.Name ?? "Sistema";
                var command = new DeleteProviderCommand
                {
                    Id = id,
                    ModificatedBy = currentUser
                };
                var result = await _mediator.Send(command);

                if (result.Succeeded)
                {
                    await ShowSuccess("Proveedor eliminado exitosamente.");
                    await LoadProviders();
                }
                else
                {
                    await ShowError(result.Message ?? "Error al eliminar proveedor");
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
            NewProviderButton.IsEnabled = !isLoading;
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

        private void RtnTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            // Solo permitir digitos
            var digits = new string(textBox.Text.Where(char.IsDigit).ToArray());

            // Limitar a 14 digitos
            if (digits.Length > 14)
                digits = digits.Substring(0, 14);

            if (textBox.Text != digits)
            {
                textBox.Text = digits;
                textBox.SelectionStart = digits.Length;
            }
        }

        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            // Solo permitir digitos
            var digits = new string(textBox.Text.Where(char.IsDigit).ToArray());

            // Limitar a 8 digitos
            if (digits.Length > 8)
                digits = digits.Substring(0, 8);

            // Formatear con guion despues de los primeros 4 digitos
            string formatted = digits.Length > 4
                ? $"{digits.Substring(0, 4)}-{digits.Substring(4)}"
                : digits;

            if (textBox.Text != formatted)
            {
                textBox.Text = formatted;
                textBox.SelectionStart = formatted.Length;
            }
        }
    }
}
