using EDA_2._0.Views;
using Microsoft.UI.Xaml;
using System;

namespace EDA_2._0
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ContentFrame.Navigate(typeof(LoginPage));
        }

        public void NavigateToPage(Type pageType)
        {
            ContentFrame.Navigate(pageType);
        }

        public void NavigateToInvoiceDetail(int invoiceId)
        {
            // Navegar a través del MainMenuPage si está activo
            if (ContentFrame.Content is MainMenuPage mainMenuPage)
            {
                mainMenuPage.NavigateToInvoiceDetail(invoiceId);
            }
        }

        public void NavigateToInvoiceCreate()
        {
            if (ContentFrame.Content is MainMenuPage mainMenuPage)
            {
                mainMenuPage.NavigateToInvoiceCreate();
            }
        }

        public void NavigateToInvoices()
        {
            if (ContentFrame.Content is MainMenuPage mainMenuPage)
            {
                mainMenuPage.NavigateToInvoices();
            }
        }

        public void NavigateToReceivables()
        {
            if (ContentFrame.Content is MainMenuPage mainMenuPage)
            {
                mainMenuPage.NavigateToReceivables();
            }
        }

        public void NavigateToCustomerReceivablesDetail(int customerId)
        {
            if (ContentFrame.Content is MainMenuPage mainMenuPage)
            {
                mainMenuPage.NavigateToCustomerReceivablesDetail(customerId);
            }
        }

        public void NavigateToPurchaseBills()
        {
            if (ContentFrame.Content is MainMenuPage mainMenuPage)
            {
                mainMenuPage.NavigateToPurchaseBills();
            }
        }

        public void NavigateToPurchaseBillDetail(int purchaseBillId)
        {
            if (ContentFrame.Content is MainMenuPage mainMenuPage)
            {
                mainMenuPage.NavigateToPurchaseBillDetail(purchaseBillId);
            }
        }

        public void NavigateToPurchaseBillEdit(int purchaseBillId)
        {
            if (ContentFrame.Content is MainMenuPage mainMenuPage)
            {
                mainMenuPage.NavigateToPurchaseBillEdit(purchaseBillId);
            }
        }
    }
}
