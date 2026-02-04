using EDA.APPLICATION.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class DatabaseSetupPage : Page
    {
        private readonly IDatabaseConfigService _configService;
        private bool _connectionTested = false;
        private string? _testedConnectionString;

        public DatabaseSetupPage()
        {
            InitializeComponent();
            _configService = App.Services.GetRequiredService<IDatabaseConfigService>();
        }

        private void DatabaseTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RadioLocalDB == null || RadioSqlServer == null) return;

            bool isLocalDB = RadioLocalDB.IsChecked == true;

            LocalDBInfoBar.IsOpen = isLocalDB;
            SqlServerPanel.Visibility = isLocalDB ? Visibility.Collapsed : Visibility.Visible;

            // Reset connection test status when changing type
            _connectionTested = false;
            _testedConnectionString = null;
            BtnContinue.IsEnabled = false;
            ConnectionStatusBar.IsOpen = false;
        }

        private void AuthTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RadioWindowsAuth == null || RadioSqlAuth == null) return;

            bool isSqlAuth = RadioSqlAuth.IsChecked == true;
            SqlCredentialsPanel.Visibility = isSqlAuth ? Visibility.Visible : Visibility.Collapsed;

            // Reset connection test status when changing auth type
            _connectionTested = false;
            _testedConnectionString = null;
            BtnContinue.IsEnabled = false;
            ConnectionStatusBar.IsOpen = false;
        }

        private async void BtnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            await TestConnectionAsync();
        }

        private async Task TestConnectionAsync()
        {
            try
            {
                SetLoading(true);
                ConnectionStatusBar.IsOpen = false;

                string connectionString = BuildConnectionString();

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    ShowConnectionStatus(false, "Por favor complete todos los campos requeridos.");
                    return;
                }

                bool success = await _configService.TestConnectionAsync(connectionString);

                if (success)
                {
                    _connectionTested = true;
                    _testedConnectionString = connectionString;
                    BtnContinue.IsEnabled = true;
                    ShowConnectionStatus(true, "Conexion exitosa. Puede continuar con la configuracion.");
                }
                else
                {
                    _connectionTested = false;
                    _testedConnectionString = null;
                    BtnContinue.IsEnabled = false;
                    ShowConnectionStatus(false, "No se pudo conectar a la base de datos. Verifique los datos e intente de nuevo.");
                }
            }
            catch (Exception ex)
            {
                _connectionTested = false;
                _testedConnectionString = null;
                BtnContinue.IsEnabled = false;
                ShowConnectionStatus(false, $"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private string BuildConnectionString()
        {
            if (RadioLocalDB.IsChecked == true)
            {
                // LocalDB connection string
                return _configService.BuildConnectionString(
                    "(localdb)\\MSSQLLocalDB",
                    "eda_db",
                    useWindowsAuth: true);
            }
            else
            {
                // SQL Server connection string
                string server = TxtServer.Text?.Trim() ?? string.Empty;
                string database = string.IsNullOrWhiteSpace(TxtDatabase.Text) ? "eda_db" : TxtDatabase.Text.Trim();
                bool useWindowsAuth = RadioWindowsAuth.IsChecked == true;

                if (string.IsNullOrWhiteSpace(server))
                {
                    return string.Empty;
                }

                if (!useWindowsAuth)
                {
                    string username = TxtUsername.Text?.Trim() ?? string.Empty;
                    string password = TxtPassword.Password ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        return string.Empty;
                    }

                    return _configService.BuildConnectionString(server, database, false, username, password);
                }

                return _configService.BuildConnectionString(server, database, true);
            }
        }

        private void ShowConnectionStatus(bool success, string message)
        {
            ConnectionStatusBar.Title = success ? "Exito" : "Error";
            ConnectionStatusBar.Message = message;
            ConnectionStatusBar.Severity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            ConnectionStatusBar.IsOpen = true;
        }

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            BtnTestConnection.IsEnabled = !isLoading;
            BtnContinue.IsEnabled = !isLoading && _connectionTested;
            DatabaseTypeSelector.IsEnabled = !isLoading;
            AuthTypeSelector.IsEnabled = !isLoading;
            TxtServer.IsEnabled = !isLoading;
            TxtDatabase.IsEnabled = !isLoading;
            TxtUsername.IsEnabled = !isLoading;
            TxtPassword.IsEnabled = !isLoading;
        }

        private async void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            if (!_connectionTested || string.IsNullOrWhiteSpace(_testedConnectionString))
            {
                ShowConnectionStatus(false, "Debe probar la conexion antes de continuar.");
                return;
            }

            try
            {
                SetLoading(true);

                // Save the connection string
                _configService.SaveConnectionString(_testedConnectionString);

                // Show success message
                var dialog = new ContentDialog
                {
                    Title = "Configuracion guardada",
                    Content = "La configuracion de la base de datos ha sido guardada. La aplicacion se reiniciara para aplicar los cambios.",
                    CloseButtonText = "Aceptar",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();

                // Restart the application
                Microsoft.Windows.AppLifecycle.AppInstance.Restart(string.Empty);
            }
            catch (Exception ex)
            {
                ShowConnectionStatus(false, $"Error al guardar la configuracion: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }
    }
}
