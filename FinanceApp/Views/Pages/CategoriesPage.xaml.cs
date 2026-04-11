using System.Windows;
using System.Windows.Controls;
using FinanceApp.Services.Categories;

namespace FinanceApp.Views.Pages
{
    public partial class CategoriesPage : UserControl
    {
        private readonly CategoryService _service = new CategoryService();

        public CategoriesPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            dgCategories.ItemsSource = _service.GetAll();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddCategoryWindow();
            if (window.ShowDialog() == true)
            {
                LoadCategories();
            }
        }
    }
}