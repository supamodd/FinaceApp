using FinanceApp.Data.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FinanceApp.Views
{
    public partial class AddCategoryWindow : Window
    {
        private string _selectedColor = "#8B5CF6";

        public AddCategoryWindow()
        {
            InitializeComponent();
            UpdateColorPreview();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void TxtColor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtColor.Text.StartsWith("#") && txtColor.Text.Length == 7)
            {
                _selectedColor = txtColor.Text.ToUpper();
                UpdateColorPreview();
            }
        }

        private void UpdateColorPreview()
        {
            try
            {
                colorPreview.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_selectedColor));
            }
            catch { }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ToastNotification.Show("Ошибка", "Введите название категории!", ToastType.Error);
                return;
            }

            var category = new Category
            {
                Name = txtName.Text.Trim(),
                Type = cmbType.SelectedIndex == 1 ? "Income" : "Expense",
                Color = _selectedColor
            };

            var service = new FinanceApp.Services.Categories.CategoryService();
            service.Add(category);

            ToastNotification.Show("Успех", $"Категория «{category.Name}» добавлена!", ToastType.Success);
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}