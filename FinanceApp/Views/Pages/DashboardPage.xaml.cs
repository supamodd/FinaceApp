using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinanceApp.Data.Models;
using FinanceApp.Services.Transactions;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FinanceApp.Views.Pages
{
    public partial class DashboardPage : UserControl
    {
        private readonly TransactionService _service = new TransactionService();
        private bool _showPercent = false;
        private DateTime? _customFrom;
        private DateTime? _customTo;

        public DashboardPage()
        {
            InitializeComponent();

            if (cmbChartMode?.SelectedIndex < 0) cmbChartMode.SelectedIndex = 0;
            if (cmbPeriod?.SelectedIndex < 0) cmbPeriod.SelectedIndex = 0;

            this.Loaded += DashboardPage_Loaded;
        }

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            if (App.CurrentUser == null) return;

            var transactions = GetFilteredTransactions();

            decimal totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            decimal totalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            decimal balance = totalIncome - totalExpense;

            txtBalance.Text = $"{balance:N0} ₽";
            txtIncome.Text = $"+{totalIncome:N0} ₽";
            txtExpense.Text = $"-{totalExpense:N0} ₽";

            if (dgLastTransactions != null)
            {
                dgLastTransactions.ItemsSource = transactions.OrderByDescending(t => t.Date).Take(8).ToList();
            }

            UpdatePieChart();
        }

        private List<FinancialTransaction> GetFilteredTransactions()
        {
            if (App.CurrentUser == null) return new List<FinancialTransaction>();

            var all = _service.GetAll(App.CurrentUser.Id);
            var selectedItem = cmbPeriod?.SelectedItem as ComboBoxItem;
            string tag = selectedItem?.Tag?.ToString() ?? "ThisMonth";

            DateTime now = DateTime.Now;

            return tag switch
            {
                "ThisMonth" => all.Where(t => t.Date.Year == now.Year && t.Date.Month == now.Month).ToList(),
                "Last30Days" => all.Where(t => t.Date >= now.AddDays(-30)).ToList(),
                "Last3Months" => all.Where(t => t.Date >= now.AddMonths(-3)).ToList(),
                "AllTime" => all.ToList(),
                "Custom" => all.Where(t =>
                    (_customFrom == null || t.Date >= _customFrom) &&
                    (_customTo == null || t.Date <= _customTo)).ToList(),
                _ => all.ToList()
            };
        }

        private void UpdatePieChart()
        {
            if (pieChart == null || App.CurrentUser == null) return;

            var transactions = GetFilteredTransactions();

            int mode = cmbChartMode?.SelectedIndex ?? 0;

            if (mode == 0)                                                                          // Доходы vs Расходы
            {
                decimal income = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
                decimal expense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);

                if (_showPercent && (income + expense) > 0)
                {
                    double total = (double)(income + expense);
                    double incomeP = (double)income / total * 100;
                    double expenseP = (double)expense / total * 100;

                    pieChart.Series = new ISeries[]
                    {
                        new PieSeries<double> { Name = $"Доходы ({incomeP:F1}%)", Values = new[] { (double)income }, Fill = new SolidColorPaint(SKColors.LimeGreen) },
                        new PieSeries<double> { Name = $"Расходы ({expenseP:F1}%)", Values = new[] { (double)expense }, Fill = new SolidColorPaint(SKColors.Red) }
                    };
                }
                else
                {
                    pieChart.Series = new ISeries[]
                    {
                        new PieSeries<double> { Name = "Доходы", Values = new[] { (double)income }, Fill = new SolidColorPaint(SKColors.LimeGreen) },
                        new PieSeries<double> { Name = "Расходы", Values = new[] { (double)expense }, Fill = new SolidColorPaint(SKColors.Red) }
                    };
                }
            }
            else                            // По категориям
            {
                var grouped = transactions
                    .Where(t => t.Category != null)
                    .GroupBy(t => t.Category)
                    .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                    .ToList();

                var series = grouped.Select(item => new PieSeries<double>
                {
                    Name = item.Category.Name,
                    Values = new[] { (double)item.Amount },
                    Fill = new SolidColorPaint(SKColor.Parse(item.Category.Color))
                }).ToArray();

                pieChart.Series = series;
            }
        }

        // ==================== Обработчики ====================

        private void CmbChartMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePieChart();
        }

        private void CmbPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPeriod?.SelectedItem is ComboBoxItem item && item.Tag?.ToString() == "Custom")
            {
                if (CustomPeriodPanel != null)
                    CustomPeriodPanel.Visibility = Visibility.Visible;
            }
            else
            {
                if (CustomPeriodPanel != null)
                    CustomPeriodPanel.Visibility = Visibility.Collapsed;
                _customFrom = null;
                _customTo = null;
            }

            LoadDashboard();
        }

        private void DpFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _customFrom = dpFrom.SelectedDate;
            LoadDashboard();
        }

        private void DpTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _customTo = dpTo.SelectedDate;
            LoadDashboard();
        }

        private void ChkPercent_Changed(object sender, RoutedEventArgs e)
        {
            _showPercent = chkPercent?.IsChecked == true;
            UpdatePieChart();
        }
    }
}