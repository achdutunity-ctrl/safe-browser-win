using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace SafeBrowser
{
    public partial class BrowserWindow : Window
    {
        private readonly Profile profile;
        private int tapCount = 0;
        private DateTime lastTap = DateTime.MinValue;

        public BrowserWindow(Profile profile)
        {
            this.profile = profile;
            InitializeComponent();
            Title = profile.Name + " — Safe Browser";
            InitWebView();
        }

        private async void InitWebView()
        {
            await WebView.EnsureCoreWebView2Async();

            // חסימת חלונות פופאפ וניווט חיצוני
            WebView.CoreWebView2.NewWindowRequested += (s, e) => e.Handled = true;
            WebView.CoreWebView2.NavigationStarting += (s, e) =>
            {
                // מאפשר ניווט רגיל בתוך האתר
            };

            // מניעת תפריט קליק ימני
            WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

            WebView.Source = new Uri(profile.Url);
        }

        // חסימת Alt+F4 וכפתור X
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (PinOverlay.Visibility == Visibility.Collapsed)
            {
                e.Cancel = true;
                ShowPinOverlay();
            }
        }

        // חסימת כפתורי מקלדת (Alt+F4, ESC וכו')
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F4 && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                ShowPinOverlay();
            }
            if (e.Key == Key.Escape && PinOverlay.Visibility == Visibility.Visible)
            {
                CancelPin();
            }
            base.OnKeyDown(e);
        }

        // 5 לחיצות בפינה = תפריט יציאה
        private void ExitTrigger_Click(object sender, MouseButtonEventArgs e)
        {
            var now = DateTime.Now;
            if ((now - lastTap).TotalSeconds > 2) tapCount = 0;
            lastTap = now;
            tapCount++;
            if (tapCount >= 5)
            {
                tapCount = 0;
                ShowPinOverlay();
            }
        }

        private void ShowPinOverlay()
        {
            PinInput.Clear();
            PinError.Text = "";
            PinOverlay.Visibility = Visibility.Visible;
            PinInput.Focus();
        }

        private void CancelPin()
        {
            PinOverlay.Visibility = Visibility.Collapsed;
            tapCount = 0;
        }

        private void CancelPin_Click(object sender, RoutedEventArgs e) => CancelPin();

        private void ConfirmPin_Click(object sender, RoutedEventArgs e) => CheckPin();

        private void PinInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) CheckPin();
        }

        private void CheckPin()
        {
            if (PinInput.Password == profile.Pin)
            {
                PinOverlay.Visibility = Visibility.Collapsed;
                Close();
            }
            else
            {
                PinError.Text = "קוד שגוי — נסה שוב";
                PinInput.Clear();
                PinInput.Focus();
            }
        }
    }
}
