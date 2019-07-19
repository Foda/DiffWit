using DiffWit.Utils;
using DiffWit.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DiffWit
{
    public sealed partial class MainPage : Page
    {
        private IDiffViewModel _currentDiffViewModel;

        private SplitDiffViewModel _splitDiffViewModel;
        private UnifiedDiffViewModel _unifiedDiffViewModel;

        private string _fileA = "";
        private string _fileB = "";

        public MainPage()
        {
            this.InitializeComponent();

            BrowseFileA.Click += BrowseFileA_Click;
            BrowseFileB.Click += BrowseFileB_Click;
        }

        private async void BrowseFileA_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            var pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile != null)
            {
                _fileA = pickedFile.Path;
            }

            await TryRefreshDiffAsync();
        }

        private async void BrowseFileB_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            var pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile != null)
            {
                _fileB = pickedFile.Path;
            }

            await TryRefreshDiffAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _unifiedDiffViewModel = new UnifiedDiffViewModel();
            _splitDiffViewModel = new SplitDiffViewModel();

            var commandLineParams = e.Parameter as ParsedCommands;
            if (commandLineParams != null)
            {
                var workingDirectory = commandLineParams.FirstOrDefault(param => param.Key == "WorkingDir").Value;
                var localFile = commandLineParams.FirstOrDefault(param => param.Key == "Local").Value;
                var remoteFile = commandLineParams.FirstOrDefault(param => param.Key == "Remote").Value;

                _fileA = localFile;
                _fileB = File.Exists(remoteFile) ? remoteFile : Path.Combine(workingDirectory, remoteFile);

                await SetDiffViewModel(_splitDiffViewModel);
            }
            else
            {
                await SetDiffViewModel(_splitDiffViewModel);
            }
        }

        private async Task SetDiffViewModel(IDiffViewModel vm)
        {
            Loading.IsActive = true;

            _currentDiffViewModel = vm;
            DataContext = _currentDiffViewModel;

            await TryRefreshDiffAsync();

            Loading.IsActive = false;
        }

        private async Task TryRefreshDiffAsync()
        {
            if (!string.IsNullOrEmpty(_fileA) && !string.IsNullOrEmpty(_fileB))
            {
                await _currentDiffViewModel.GenerateDiffAsync(_fileA, _fileB);
            }
        }

        private async void MenuFlyoutItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var flyoutItem = (MenuFlyoutItem)sender;

            var option = flyoutItem.Tag.ToString();
            if (option == "split")
            {
                SelectedViewIcon.Glyph = "\uF30A";
                SelectedViewText.Text = "Split";

                await SetDiffViewModel(_splitDiffViewModel);
            }
            else if (option == "unified")
            {
                SelectedViewIcon.Glyph = "\uF309";
                SelectedViewText.Text = "Unified";

                await SetDiffViewModel(_unifiedDiffViewModel);
            }
        }
    }
}