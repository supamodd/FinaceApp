using System;
using System.Windows;
using System.Windows.Controls;
using FinanceApp.Views.Pages;

namespace FinanceApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ContentFrame.Navigate(new DashboardPage());
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new DashboardPage());
        }

        private void Transactions_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new TransactionsPage());
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new CategoriesPage());
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ReportsPage());
        }

        private void Budgets_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new BudgetsPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}