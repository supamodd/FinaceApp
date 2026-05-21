using FinanceApp.Data;
using FinanceApp.Services;
using FinanceApp.Views;
using FinanceApp.Views.Pages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FinanceApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigateTo(new DashboardPage(), btnDashboard);
            this.MouseLeftButtonDown += (s, e) => { if (e.ChangedButton == MouseButton.Left) DragMove(); };
        }

        // ── Навигация с анимацией ──
        private void NavigateTo(object page, Button activeBtn)
        {
            // Fade-out
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(120));
            fadeOut.Completed += (s, e) =>
            {
                ContentFrame.Navigate(page);
                SetActiveButton(activeBtn);

                // Fade-in + slide up
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
                var slideUp = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                ContentFrame.BeginAnimation(OpacityProperty, fadeIn);

                var transform = new TranslateTransform();
                ContentFrame.RenderTransform = transform;
                transform.BeginAnimation(TranslateTransform.YProperty, slideUp);
            };

            ContentFrame.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void SetActiveButton(Button activeBtn)
        {
            foreach (var btn in new[] { btnDashboard, btnTransactions, btnCategories, btnBudgets, btnReports, btnFamily })
            {
                btn.Tag = null;
            }
            activeBtn.Tag = "Active";
        }

        // ── Window chrome ──
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnMaximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        // ── Navigation handlers ──
        private void Dashboard_Click(object sender, RoutedEventArgs e) => NavigateTo(new DashboardPage(), btnDashboard);
        private void Transactions_Click(object sender, RoutedEventArgs e) => NavigateTo(new TransactionsPage(), btnTransactions);
        private void Categories_Click(object sender, RoutedEventArgs e) => NavigateTo(new CategoriesPage(), btnCategories);
        private void Budgets_Click(object sender, RoutedEventArgs e) => NavigateTo(new BudgetsPage(), btnBudgets);
        private void Reports_Click(object sender, RoutedEventArgs e) => NavigateTo(new ReportsPage(), btnReports);
        private void Family_Click(object sender, RoutedEventArgs e) => NavigateTo(new FamilyPage(), btnFamily);

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private async void TestFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();
            var familyService = new FamilyService(context);
            var family = await familyService.CreateFamilyAsync(1, "Тестовая Семья");
            MessageBox.Show($"Семья создана! ID = {family.Id}, Название: {family.Name}");
        }
    }
}