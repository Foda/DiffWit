using DiffWit.Utils;
using DiffWit.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TextEditor.Diff;
using TextEditor.Utils;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DiffWit
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IDiffViewModel _currentDiffViewModel;

        private SplitDiffViewModel _splitDiffViewModel;
        private UnifiedDiffViewModel _unifiedDiffViewModel;

        private List<Diff> _diffCache = new List<Diff>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var commandLineParams = e.Parameter as ParsedCommands;
            if (commandLineParams != null)
            {
                var workingDirectory = commandLineParams.FirstOrDefault(param => param.Key == "WorkingDir").Value;
                var localFile = commandLineParams.FirstOrDefault(param => param.Key == "Local").Value;
                var remoteFile = commandLineParams.FirstOrDefault(param => param.Key == "Remote").Value;

                var fileA = localFile;
                var fileB = File.Exists(remoteFile) ? remoteFile : Path.Combine(workingDirectory, remoteFile);

                await GenerateDiffCache(fileA, fileB);

                _unifiedDiffViewModel = new UnifiedDiffViewModel(fileA, fileB, _diffCache);
                _splitDiffViewModel = new SplitDiffViewModel(fileA, fileB, _diffCache);
                
                SetDiffViewModel(_splitDiffViewModel);
            }
        }

        private async Task GenerateDiffCache(string fileA, string fileB)
        {
            fileA = fileA.Replace("/", "\\");
            fileB = fileB.Replace("/", "\\");

            var fileAFolder = await StorageFolder.GetFolderFromPathAsync(
                fileA.Substring(0, fileA.LastIndexOf('\\')));

            var fileBFolder = await StorageFolder.GetFolderFromPathAsync(
                fileB.Substring(0, fileB.LastIndexOf('\\')));

            var fileAFile = await fileAFolder.GetFileAsync(FileHelper.GetFileName(fileA));
            var fileBFile = await fileBFolder.GetFileAsync(FileHelper.GetFileName(fileB));

            var fileAText = await Windows.Storage.FileIO.ReadTextAsync(fileAFile);
            var fileBText = await Windows.Storage.FileIO.ReadTextAsync(fileBFile);

            fileAText = fileAText.Replace("\r\n", "\n");
            fileBText = fileBText.Replace("\r\n", "\n");

            _diffCache = DiffFactory.GenerateDiffCache(fileAText, fileBText);
        }

        private void SetDiffViewModel(IDiffViewModel vm)
        {
            _currentDiffViewModel = vm;
            DataContext = _currentDiffViewModel;

            _currentDiffViewModel.ProcessDiff();
        }

        private void MenuFlyoutItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var option = ((MenuFlyoutItem)sender).Tag.ToString();
            if (option == "split")
            {
                SetDiffViewModel(_splitDiffViewModel);
            }
            else if (option == "unified")
            {
                SetDiffViewModel(_unifiedDiffViewModel);
            }
        }
    }
}
