using EDA.APPLICATION.Features.UserFeature.Commands.LoginCommand;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class LoginPage : Page
    {
        private readonly IMediator _mediator;

        public LoginPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
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
                    App.MainWindow.NavigateToPage(typeof(MainMenuPage));
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

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
