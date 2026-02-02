using EDA.APPLICATION.Features.ShiftFeature.Commands.CreateShiftCommand;
using EDA.APPLICATION.Features.UserFeature.Commands.LoginCommand;
using EDA.DOMAIN.Entities;
using EDA.INFRAESTRUCTURE;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class LoginPage : Page
    {
        private readonly IMediator _mediator;
        private readonly DatabaseContext _dbContext;

        public LoginPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
            _dbContext = App.Services.GetRequiredService<DatabaseContext>();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await AttemptLogin();
        }

        private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await AttemptLogin();
            }
        }

        private async Task AttemptLogin()
        {
            ErrorMessage.Visibility = Visibility.Collapsed;
            LoginButton.IsEnabled = false;
            LoadingRing.IsActive = true;

            try
            {
                var command = new LoginCommand
                {
                    UserName = UsernameTextBox.Text?.Trim() ?? string.Empty,
                    Password = PasswordBox.Password ?? string.Empty
                };

                var result = await _mediator.Send(command);

                if (result.Succeeded && result.Data != null)
                {
                    App.CurrentUser = result.Data;

                    // Verificar si tiene turno abierto
                    var openShift = await _dbContext.Shifts
                        .FirstOrDefaultAsync(s => s.UserId == result.Data.Id && s.IsOpen);

                    if (openShift != null)
                    {
                        App.CurrentShift = openShift;
                        App.MainWindow.NavigateToPage(typeof(MainMenuPage));
                    }
                    else
                    {
                        await ShowOpenShiftDialog(result.Data);
                    }
                }
                else
                {
                    ShowError(result.Message ?? "Error al iniciar sesion");
                }
            }
            catch (EDA.APPLICATION.Exceptions.ValidationException vex)
            {
                ShowError(string.Join("\n", vex.Errors));
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoadingRing.IsActive = false;
            }
        }

        private async Task ShowOpenShiftDialog(User user)
        {
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
                Children = { shiftTypeComboBox, initialAmountBox }
            };

            while (true)
            {
                var dialog = new ContentDialog
                {
                    Title = $"Abrir Turno - {user.Name} {user.LastName}",
                    Content = content,
                    PrimaryButtonText = "Abrir Turno",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();

                // Validar selección
                if (shiftTypeComboBox.SelectedItem == null)
                {
                    shiftTypeComboBox.Header = "Tipo de turno * (requerido)";
                    continue;
                }

                var shiftType = shiftTypeComboBox.SelectedItem.ToString()!;
                decimal initialAmount = double.IsNaN(initialAmountBox.Value) ? 0 : (decimal)initialAmountBox.Value;

                try
                {
                    var createCommand = new CreateShiftCommand
                    {
                        UserId = user.Id,
                        ShiftType = shiftType,
                        InitialAmount = initialAmount
                    };

                    var result = await _mediator.Send(createCommand);

                    if (result.Succeeded && result.Data != null)
                    {
                        App.CurrentShift = result.Data;
                        App.MainWindow.NavigateToPage(typeof(MainMenuPage));
                        return;
                    }
                    else
                    {
                        ShowError(result.Message ?? "Error al abrir turno");
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Error: {ex.Message}");
                }

                break;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
