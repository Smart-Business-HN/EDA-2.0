using EDA.APPLICATION.Features.RoleFeature.Queries;
using EDA.APPLICATION.Features.UserFeature.Commands.CreateUserCommand;
using EDA.APPLICATION.Features.UserFeature.Commands.UpdateUserCommand;
using EDA.APPLICATION.Features.UserFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class UsersPage : Page
    {
        private readonly IMediator _mediator;
        private List<Role> _roles = new();
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public UsersPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRoles();
            await LoadUsers();
        }

        private async Task LoadRoles()
        {
            try
            {
                var result = await _mediator.Send(new GetAllRolesQuery());
                if (result.Succeeded && result.Data != null)
                {
                    _roles = result.Data;
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error al cargar roles: {ex.Message}");
            }
        }

        private async Task LoadUsers()
        {
            SetLoading(true);

            try
            {
                var query = new GetAllUsersQuery
                {
                    SearchTerm = SearchTextBox.Text?.Trim(),
                    PageNumber = _currentPage,
                    PageSize = _pageSize
                };

                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    UsersListView.ItemsSource = result.Data.Items;
                    _totalPages = result.Data.TotalPages > 0 ? result.Data.TotalPages : 1;
                    UpdatePaginationUI(result.Data.HasPreviousPage, result.Data.HasNextPage);
                }
                else
                {
                    await ShowError(result.Message ?? "Error al cargar usuarios");
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
                await LoadUsers();
            }
        }

        private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadUsers();
            }
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadUsers();
            }
        }

        private async void NewUserButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowUserDialog(null);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is User user)
            {
                await ShowUserDialog(user);
            }
        }

        private async Task ShowUserDialog(User? user)
        {
            bool isEdit = user != null;

            var nameTextBox = new TextBox
            {
                Header = "Nombre de usuario *",
                PlaceholderText = "Ingrese el nombre de usuario",
                Text = user?.Name ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var lastNameTextBox = new TextBox
            {
                Header = "Apellido *",
                PlaceholderText = "Ingrese el apellido",
                Text = user?.LastName ?? string.Empty,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var passwordBox = new PasswordBox
            {
                Header = isEdit ? "Contraseña (dejar vacío para no cambiar)" : "Contraseña *",
                PlaceholderText = "Ingrese la contraseña",
                Margin = new Thickness(0, 0, 0, 12)
            };

            var roleComboBox = new ComboBox
            {
                Header = "Rol *",
                PlaceholderText = "Seleccione un rol",
                ItemsSource = _roles,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 12)
            };

            if (user != null)
            {
                roleComboBox.SelectedValue = user.RoleId;
            }

            var content = new StackPanel
            {
                Width = 350,
                Children =
                {
                    nameTextBox,
                    lastNameTextBox,
                    passwordBox,
                    roleComboBox
                }
            };

            var dialog = new ContentDialog
            {
                Title = isEdit ? "Editar Usuario" : "Nuevo Usuario",
                Content = content,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await SaveUser(
                    user?.Id,
                    nameTextBox.Text?.Trim() ?? string.Empty,
                    lastNameTextBox.Text?.Trim() ?? string.Empty,
                    passwordBox.Password,
                    (int?)roleComboBox.SelectedValue ?? 0,
                    isEdit);
            }
        }

        private async Task SaveUser(int? id, string name, string lastName, string password, int roleId, bool isEdit)
        {
            SetLoading(true);

            try
            {
                if (isEdit && id.HasValue)
                {
                    var command = new UpdateUserCommand
                    {
                        Id = id.Value,
                        Name = name,
                        LastName = lastName,
                        Password = string.IsNullOrWhiteSpace(password) ? null : password,
                        RoleId = roleId
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Usuario actualizado exitosamente.");
                        await LoadUsers();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al actualizar usuario");
                    }
                }
                else
                {
                    var command = new CreateUserCommand
                    {
                        Name = name,
                        LastName = lastName,
                        Password = password,
                        RoleId = roleId
                    };

                    var result = await _mediator.Send(command);

                    if (result.Succeeded)
                    {
                        await ShowSuccess("Usuario creado exitosamente.");
                        await LoadUsers();
                    }
                    else
                    {
                        await ShowError(result.Message ?? "Error al crear usuario");
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

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            NewUserButton.IsEnabled = !isLoading;
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
