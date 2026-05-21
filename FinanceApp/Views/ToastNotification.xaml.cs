using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FinanceApp.Views
{
    public enum ToastType
    {
        Success,
        Error,
        Warning,
        Info
    }

    public partial class ToastNotification : UserControl
    {
        private static StackPanel? _container;
        private Action? _onConfirm;
        private Action? _onCancel;
        private bool _isConfirmMode = false;

        public ToastNotification()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Вызывайте один раз при старте главного окна
        /// </summary>
        public static void Initialize(Grid parentGrid)
        {
            _container = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 20, 20)
            };
            Grid.SetColumnSpan(_container, 10);
            Grid.SetRowSpan(_container, 10);
            parentGrid.Children.Add(_container);
        }

        /// <summary>
        /// Обычное уведомление (исчезает автоматически)
        /// </summary>
        public static void Show(string title, string message, ToastType type = ToastType.Info, int durationMs = 3500)
        {
            if (_container == null) return;

            var toast = new ToastNotification();
            toast.Configure(title, message, type);
            _container.Children.Add(toast);
            toast.AnimateIn(durationMs);
        }

        /// <summary>
        /// Уведомление с подтверждением (Да/Отмена) — не исчезает автоматически
        /// </summary>
        public static void ShowConfirm(string title, string message, Action onConfirm,
            string confirmText = "Да, удалить", Action? onCancel = null)
        {
            if (_container == null) return;

            var toast = new ToastNotification();
            toast._isConfirmMode = true;
            toast._onConfirm = onConfirm;
            toast._onCancel = onCancel;
            toast.Configure(title, message, ToastType.Warning);
            toast.ConfirmPanel.Visibility = Visibility.Visible;
            toast.btnConfirm.Content = confirmText;
            toast.btnCancel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            _container.Children.Add(toast);
            toast.AnimateIn(-1); // -1 = не исчезает автоматически
        }

        private void Configure(string title, string message, ToastType type)
        {
            txtTitle.Text = title;
            txtMessage.Text = message;

            switch (type)
            {
                case ToastType.Success:
                    txtIcon.Text = "✅";
                    ToastBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34D399"));
                    ToastBorder.Background = CreateGradient("#0D2818", "#0D1A2A");
                    break;

                case ToastType.Error:
                    txtIcon.Text = "❌";
                    ToastBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171"));
                    ToastBorder.Background = CreateGradient("#2A0D0D", "#1A0D1A");
                    break;

                case ToastType.Warning:
                    txtIcon.Text = "⚠️";
                    ToastBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"));
                    ToastBorder.Background = CreateGradient("#2A2200", "#1A1500");
                    break;

                case ToastType.Info:
                default:
                    txtIcon.Text = "💜";
                    ToastBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B5CF6"));
                    ToastBorder.Background = CreateGradient("#1E1A3A", "#151229");
                    break;
            }
        }

        private LinearGradientBrush CreateGradient(string color1, string color2)
        {
            return new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString(color1),
                (Color)ColorConverter.ConvertFromString(color2),
                45);
        }

        private async void AnimateIn(int durationMs)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            var slideIn = new DoubleAnimation(380, 0, TimeSpan.FromMilliseconds(350)) { EasingFunction = ease };
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(350));

            SlideTransform.BeginAnimation(TranslateTransform.XProperty, slideIn);
            this.BeginAnimation(OpacityProperty, fadeIn);

            // Если -1, не исчезает (ждёт действия пользователя)
            if (durationMs > 0)
            {
                await Task.Delay(durationMs);
                AnimateOut();
            }
        }

        private void AnimateOut()
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseIn };
            var slideOut = new DoubleAnimation(0, 380, TimeSpan.FromMilliseconds(300)) { EasingFunction = ease };
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));

            fadeOut.Completed += (s, e) =>
            {
                _container?.Children.Remove(this);
            };

            SlideTransform.BeginAnimation(TranslateTransform.XProperty, slideOut);
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_isConfirmMode) _onCancel?.Invoke();
            AnimateOut();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            _onConfirm?.Invoke();
            AnimateOut();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _onCancel?.Invoke();
            AnimateOut();
        }
    }
}