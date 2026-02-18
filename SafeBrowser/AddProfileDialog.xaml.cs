using System.Windows;

namespace SafeBrowser
{
    public partial class AddProfileDialog : Window
    {
        public Profile NewProfile { get; private set; } = new();

        public AddProfileDialog()
        {
            InitializeComponent();
            TbName.Focus();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var name = TbName.Text.Trim();
            var url = TbUrl.Text.Trim();
            var pin = PbPin.Password;
            var pinConfirm = PbPinConfirm.Password;

            if (string.IsNullOrEmpty(name)) { ErrorMsg.Text = "נא להכניס שם פרופיל"; return; }
            if (string.IsNullOrEmpty(url) || url == "https://") { ErrorMsg.Text = "נא להכניס כתובת אתר"; return; }
            if (pin.Length < 4) { ErrorMsg.Text = "הקוד חייב להכיל לפחות 4 ספרות"; return; }
            if (pin != pinConfirm) { ErrorMsg.Text = "הקודים אינם תואמים"; return; }

            if (!url.StartsWith("http")) url = "https://" + url;

            NewProfile = new Profile { Name = name, Url = url, Pin = pin };
            DialogResult = true;
        }
    }
}
