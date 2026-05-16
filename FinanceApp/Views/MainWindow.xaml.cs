using FinanceApp.Data;
using FinanceApp.Services;
using FinanceApp.Views.Pages;
using System;
using System.Windows;
using System.Windows.Controls;

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

        // Временный тест (потом удалишь)
        private async void TestFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();                    // создаём контекст
            var familyService = new FamilyService(context);      // создаём сервис

            // Создаём тестовую семью (замени 1 на ID твоего текущего пользователя)
            var family = await familyService.CreateFamilyAsync(1, "Тестовая Семья");

            MessageBox.Show($"Семья создана! ID = {family.Id}, Название: {family.Name}");
        }
    }
}