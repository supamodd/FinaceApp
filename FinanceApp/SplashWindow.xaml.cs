using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FinanceApp.Views
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            this.Loaded += SplashWindow_Loaded;
        }

        private async void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var duration200 = TimeSpan.FromMilliseconds(200);
            var duration400 = TimeSpan.FromMilliseconds(400);
            var duration600 = TimeSpan.FromMilliseconds(600);
            var duration1500 = TimeSpan.FromMilliseconds(1500);
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            // 1. Логотип — fade in + slide up
            await Task.Delay(200);
            AnimateOpacity(imgLogo, 0, 1, duration600);
            AnimateDouble(LogoTranslate, TranslateTransform.YProperty, 30, 0, duration600, ease);

            // 2. Glow pulse
            AnimateOpacity(GlowPulse, 0, 0.1, duration600);

            // 3. Заголовок — fade in + slide up
            await Task.Delay(300);
            AnimateOpacity(txtTitle, 0, 1, duration400);
            AnimateDouble(TitleTranslate, TranslateTransform.YProperty, 20, 0, duration400, ease);

            // 4. Подзаголовок
            await Task.Delay(200);
            AnimateOpacity(txtSubtitle, 0, 1, duration400);

            // 5. Версия
            await Task.Delay(100);
            AnimateOpacity(txtVersion, 0, 1, duration200);

            // 6. Прогресс-бар
            var progressAnim = new DoubleAnimation(0, 200, duration1500)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            ProgressBar.BeginAnimation(WidthProperty, progressAnim);

            // 7. Ждём завершения и переходим
            await Task.Delay(1800);

            // 8. Fade out splash
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s2, e2) =>
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            };
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void AnimateOpacity(UIElement element, double from, double to, TimeSpan duration)
        {
            var anim = new DoubleAnimation(from, to, duration);
            element.BeginAnimation(OpacityProperty, anim);
        }

        private void AnimateDouble(Animatable target, DependencyProperty prop, double from, double to, TimeSpan duration, IEasingFunction? easing = null)
        {
            var anim = new DoubleAnimation(from, to, duration);
            if (easing != null) anim.EasingFunction = easing;
            target.BeginAnimation(prop, anim);
        }
    }
}