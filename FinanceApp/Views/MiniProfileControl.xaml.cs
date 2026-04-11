using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FinanceApp.Data.Models;

namespace FinanceApp.Views
{
    public partial class MiniProfileControl : UserControl
    {
        public MiniProfileControl()
        {
            InitializeComponent();
            Loaded += MiniProfileControl_Loaded;
        }

        private void MiniProfileControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateProfile();
        }

        public void UpdateProfile()
        {
            if (App.CurrentUser == null) return;

            txtFullName.Text = App.CurrentUser.FullName ?? "Пользователь";
            txtLogin.Text = "@" + (App.CurrentUser.Login ?? "—");

            // Загрузка аватарки
            if (!string.IsNullOrEmpty(App.CurrentUser.AvatarPath) && System.IO.File.Exists(App.CurrentUser.AvatarPath))
            {
                imgAvatar.Source = new BitmapImage(new Uri(App.CurrentUser.AvatarPath));
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;

            var window = new EditProfileWindow(App.CurrentUser);
            if (window.ShowDialog() == true)
            {
                UpdateProfile();        // обновляем мини-профиль после изменений
            }
        }
    }
}