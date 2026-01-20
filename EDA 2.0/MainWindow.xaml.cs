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
    }
}
