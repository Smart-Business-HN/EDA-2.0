using EDA.DOMAIN.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace EDA_2._0.Views
{
    public sealed partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
            ConfigureMenuByRole();
        }

        private void ConfigureMenuByRole()
        {
            var currentUser = App.CurrentUser;
            if (currentUser == null) return;

            var role = (RoleEnum)currentUser.RoleId;

            if (role == RoleEnum.Cajero)
            {
                // Cajero solo tiene acceso a: POS, Clientes, Productos
                // Ocultar items no permitidos
                NavFamilias.Visibility = Visibility.Collapsed;
                NavImpuestos.Visibility = Visibility.Collapsed;
                NavDescuentos.Visibility = Visibility.Collapsed;
                NavFacturas.Visibility = Visibility.Collapsed;
                NavTiposPago.Visibility = Visibility.Collapsed;
                NavCAIs.Visibility = Visibility.Collapsed;
                NavUsuarios.Visibility = Visibility.Collapsed;
                NavEmpresa.Visibility = Visibility.Collapsed;
                HeaderConfiguracion.Visibility = Visibility.Collapsed;
            }
            // Admin tiene acceso a todo (por defecto visible)
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                var tag = selectedItem.Tag?.ToString();

                switch (tag)
                {
                    case "pos":
                        ContentFrame.Navigate(typeof(POSPage));
                        break;
                    case "empresa":
                        ContentFrame.Navigate(typeof(CompanyPage));
                        break;
                    case "usuarios":
                        ContentFrame.Navigate(typeof(UsersPage));
                        break;
                    case "familias":
                        ContentFrame.Navigate(typeof(FamiliesPage));
                        break;
                    case "tipospago":
                        ContentFrame.Navigate(typeof(PaymentTypesPage));
                        break;
                    case "descuentos":
                        ContentFrame.Navigate(typeof(DiscountsPage));
                        break;
                    case "impuestos":
                        ContentFrame.Navigate(typeof(TaxesPage));
                        break;
                    case "clientes":
                        ContentFrame.Navigate(typeof(CustomersPage));
                        break;
                    case "productos":
                        ContentFrame.Navigate(typeof(ProductsPage));
                        break;
                    case "cais":
                        ContentFrame.Navigate(typeof(CaisPage));
                        break;
                    case "facturas":
                        ContentFrame.Navigate(typeof(InvoicesPage));
                        break;
                }
            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // ItemInvoked se dispara siempre, incluso con SelectsOnInvoked="False"
            if (args.InvokedItemContainer is NavigationViewItem invokedItem)
            {
                var tag = invokedItem.Tag?.ToString();

                // Si es el item de configuracion de usuario, mostrar flyout
                if (tag == "usersettings")
                {
                    FlyoutBase.ShowAttachedFlyout(invokedItem);
                }
            }
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Cambiar Contrasena",
                Content = "Esta funcionalidad sera implementada proximamente.",
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            App.MainWindow.NavigateToPage(typeof(LoginPage));
        }

        public void NavigateToInvoiceDetail(int invoiceId)
        {
            var detailPage = new InvoiceDetailPage();
            detailPage.SetInvoiceId(invoiceId);
            ContentFrame.Navigate(typeof(InvoiceDetailPage), invoiceId);
            ContentFrame.Content = detailPage;
        }

        public void NavigateToInvoices()
        {
            ContentFrame.Navigate(typeof(InvoicesPage));
            NavView.SelectedItem = NavFacturas;
        }
    }
}
