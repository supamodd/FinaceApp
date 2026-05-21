using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinanceApp.Data.Models;
using FinanceApp.Services.Transactions;
using OfficeOpenXml;

namespace FinanceApp.Views.Pages
{
    public partial class ReportsPage : UserControl
    {
        private readonly TransactionService _service = new TransactionService();

        public ReportsPage()
        {
            InitializeComponent();
            dpFrom.SelectedDate = DateTime.Now.AddMonths(-1);
            dpTo.SelectedDate = DateTime.Now;
            BtnShowReport_Click(null, null);
        }

        private void BtnShowReport_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;

            DateTime from = dpFrom.SelectedDate ?? DateTime.Now.AddMonths(-1);
            DateTime to = dpTo.SelectedDate ?? DateTime.Now;

            var transactions = _service.GetAll(App.CurrentUser.Id)
                .Where(t => t.Date.Date >= from.Date && t.Date.Date <= to.Date)
                .OrderByDescending(t => t.Date).ToList();

            dgReport.ItemsSource = transactions;

            decimal income = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            decimal expense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            txtTotalIncome.Text = $"+{income:N0} ₽";
            txtTotalExpense.Text = $"-{expense:N0} ₽";
            txtTotalBalance.Text = $"{income - expense:N0} ₽";
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dgReport.ItemsSource == null)
            {
                ToastNotification.Show("Внимание", "Сначала сформируйте отчёт", ToastType.Warning);
                return;
            }

            var transactions = ((System.Collections.IEnumerable)dgReport.ItemsSource)
                .Cast<FinancialTransaction>().ToList();

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Финансовый отчёт");

                worksheet.Cells[1, 1].Value = "Дата";
                worksheet.Cells[1, 2].Value = "Тип";
                worksheet.Cells[1, 3].Value = "Сумма";
                worksheet.Cells[1, 4].Value = "Категория";
                worksheet.Cells[1, 5].Value = "Описание";

                int row = 2;
                foreach (var t in transactions)
                {
                    worksheet.Cells[row, 1].Value = t.Date;
                    worksheet.Cells[row, 2].Value = t.Type == "Income" ? "Доход" : "Расход";
                    worksheet.Cells[row, 3].Value = t.Amount;
                    worksheet.Cells[row, 4].Value = t.Category?.Name ?? "";
                    worksheet.Cells[row, 5].Value = t.Description;
                    row++;
                }

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Отчёт_{DateTime.Now:yyyy-MM-dd}.xlsx",
                    Filter = "Excel файлы (*.xlsx)|*.xlsx"
                };

                if (dialog.ShowDialog() == true)
                {
                    package.SaveAs(new System.IO.FileInfo(dialog.FileName));
                    ToastNotification.Show("Успех", "Отчёт сохранён в Excel!", ToastType.Success);
                }
            }
            catch (Exception ex)
            {
                ToastNotification.Show("Ошибка", $"Ошибка при экспорте: {ex.Message}", ToastType.Error);
            }
        }
    }
}