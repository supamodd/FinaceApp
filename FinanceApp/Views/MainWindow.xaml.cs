using FinanceApp.Data;
using FinanceApp.Services;
using FinanceApp.Views;
using FinanceApp.Views.Pages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FinanceApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ContentFrame.Navigate(new DashboardPage());
            // Allow dragging the borderless window
            this.MouseLeftButtonDown += (s, e) => { if (e.ChangedButton == MouseButton.Left) DragMove(); };
        }

        // ── Window chrome buttons ──
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        // ── Navigation ──
        private void Dashboard_Click(object sender, RoutedEventArgs e) => ContentFrame.Navigate(new DashboardPage());
        private void Transactions_Click(object sender, RoutedEventArgs e) => ContentFrame.Navigate(new TransactionsPage());
        private void Categories_Click(object sender, RoutedEventArgs e) => ContentFrame.Navigate(new CategoriesPage());
        private void Reports_Click(object sender, RoutedEventArgs e) => ContentFrame.Navigate(new ReportsPage());
        private void Budgets_Click(object sender, RoutedEventArgs e) => ContentFrame.Navigate(new BudgetsPage());
        private void Family_Click(object sender, RoutedEventArgs e) => ContentFrame.Navigate(new FamilyPage());

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
            var context = new AppDbContext();
            var familyService = new FamilyService(context);
            var family = await familyService.CreateFamilyAsync(1, "Тестовая Семья");
            MessageBox.Show($"Семья создана! ID = {family.Id}, Название: {family.Name}");
        }
    }
}
