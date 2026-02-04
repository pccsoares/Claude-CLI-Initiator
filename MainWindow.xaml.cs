using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ClaudeCliInitiator.Models;
using ClaudeCliInitiator.Services;

namespace ClaudeCliInitiator;

public partial class MainWindow : Window
{
    private readonly FolderScanner _folderScanner;
    private readonly ScoreManager _scoreManager;
    private readonly TerminalLauncher _terminalLauncher;
    private readonly TerminalSettingsManager _terminalSettingsManager;
    private List<RepoFolder> _folders;
    private ICollectionView _foldersView;

    public MainWindow()
    {
        InitializeComponent();

        _folderScanner = new FolderScanner();
        _scoreManager = new ScoreManager();
        _terminalLauncher = new TerminalLauncher();
        _terminalSettingsManager = new TerminalSettingsManager();
        _folders = new List<RepoFolder>();
        _foldersView = CollectionViewSource.GetDefaultView(_folders);

        LoadFolders();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SearchBox.Focus();
        CheckTerminalSettings();
    }

    private void CheckTerminalSettings()
    {
        if (!_terminalSettingsManager.SettingsFileExists)
            return;

        if (_terminalSettingsManager.IsSuppressApplicationTitleEnabled())
            return;

        var result = MessageBox.Show(
            "Windows Terminal's 'suppressApplicationTitle' setting is not enabled.\n\n" +
            "This setting is required for the tab title feature to work properly.\n\n" +
            "Would you like to enable it now?",
            "Windows Terminal Settings",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            if (_terminalSettingsManager.EnableSuppressApplicationTitle())
            {
                MessageBox.Show(
                    "Setting enabled successfully.\n\n" +
                    "Please restart any open Windows Terminal windows for the change to take effect.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "Failed to update Windows Terminal settings.\n\n" +
                    "You may need to manually add '\"suppressApplicationTitle\": true' to your settings.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void LoadFolders()
    {
        _folders = _folderScanner.GetFolders();
        _scoreManager.ApplyScores(_folders);

        _foldersView = CollectionViewSource.GetDefaultView(_folders);
        _foldersView.SortDescriptions.Clear();
        _foldersView.SortDescriptions.Add(new SortDescription("Score", ListSortDirection.Descending));
        _foldersView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

        FolderList.ItemsSource = _foldersView;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_foldersView == null) return;

        var searchText = SearchBox.Text.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(searchText))
        {
            _foldersView.Filter = null;
        }
        else
        {
            _foldersView.Filter = obj =>
            {
                if (obj is RepoFolder folder)
                {
                    return folder.Name.ToLowerInvariant().Contains(searchText);
                }
                return false;
            };
        }
    }

    private void FolderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        LaunchSelectedFolder();
    }

    private void FolderList_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            LaunchSelectedFolder();
        }
    }

    private void LaunchSelectedFolder()
    {
        if (FolderList.SelectedItem is RepoFolder folder)
        {
            _scoreManager.IncrementScore(folder.Name);
            folder.Score = _scoreManager.GetScore(folder.Name);

            _foldersView.Refresh();

            _terminalLauncher.Launch(folder.FullPath, folder.Name);
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        var searchText = SearchBox.Text;
        LoadFolders();
        SearchBox.Text = searchText;
        SearchBox_TextChanged(SearchBox, null!);
    }
}

public class LengthToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int length)
        {
            return length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
