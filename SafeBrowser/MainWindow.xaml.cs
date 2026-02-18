using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace SafeBrowser
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Profile> profiles = new();
        private readonly string dataFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SafeBrowser", "profiles.json");

        public ICommand DeleteCommand { get; }

        public MainWindow()
        {
            InitializeComponent();
            DeleteCommand = new RelayCommand<Profile>(DeleteProfile);
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            if (File.Exists(dataFile))
            {
                var json = File.ReadAllText(dataFile);
                profiles = JsonConvert.DeserializeObject<ObservableCollection<Profile>>(json) 
                           ?? new ObservableCollection<Profile>();
            }
            ProfilesList.ItemsSource = profiles;
            EmptyMessage.Visibility = profiles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveProfiles()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dataFile)!);
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(profiles));
        }

        private void AddProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProfileDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                profiles.Add(dialog.NewProfile);
                SaveProfiles();
                EmptyMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void ProfileCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is Profile profile)
            {
                var browser = new BrowserWindow(profile);
                browser.Show();
            }
        }

        private void DeleteProfile(Profile? profile)
        {
            if (profile == null) return;
            var result = MessageBox.Show(
                $"למחוק את \"{profile.Name}\"?",
                "מחיקת פרופיל",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                profiles.Remove(profile);
                SaveProfiles();
                EmptyMessage.Visibility = profiles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> execute;
        public RelayCommand(Action<T?> execute) => this.execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter is T t ? t : default);
        public event EventHandler? CanExecuteChanged;
    }
}
