using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiffWit.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

//https://github.com/microsoft/microsoft-ui-xaml/issues/4100
using WinRT;

namespace DiffWit.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private MainWindow _mainWindow;

        private IDiffViewModel _currentDiffViewModel;
        public IDiffViewModel CurrentDiffViewModel
        {
            get => _currentDiffViewModel;
            private set => SetProperty(ref _currentDiffViewModel, value);
        }

        private string _fileA;
        public string FileA
        {
            get => _fileA;
            private set
            {
                SetProperty(ref _fileA, value);
                OnPropertyChanged(nameof(HasValidDiff));
            }
        }

        private string _fileAName;
        public string FileAName
        {
            get => _fileAName;
            private set => SetProperty(ref _fileAName, value);
        }

        private string _fileB;
        public string FileB
        {
            get => _fileB;
            private set
            {
                SetProperty(ref _fileB, value);
                OnPropertyChanged(nameof(HasValidDiff));
            }
        }

        private string _fileBName;
        public string FileBName
        {
            get => _fileBName;
            private set => SetProperty(ref _fileBName, value);
        }

        public bool HasValidDiff => !string.IsNullOrEmpty(FileA) && !string.IsNullOrEmpty(FileB);

        public AsyncRelayCommand BrowseForFileA { get; }
        public AsyncRelayCommand BrowseForFileB { get; }
        public AsyncRelayCommand SwapFiles { get; }

        private SplitDiffViewModel _splitDiffViewModel;
        private UnifiedDiffViewModel _unifiedDiffViewModel;

        public MainWindowViewModel(MainWindow parentWindow)
        {
            _mainWindow = parentWindow;

            _unifiedDiffViewModel = new UnifiedDiffViewModel();
            _splitDiffViewModel = new SplitDiffViewModel();

            CurrentDiffViewModel = _splitDiffViewModel;

            BrowseForFileA = new AsyncRelayCommand(
                async () =>
                {
                    string newFile = await BrowseForFile();

                    if (!string.IsNullOrEmpty(newFile))
                    {
                        await RefreshDiff(newFile, FileB);
                    }
                });

            BrowseForFileB = new AsyncRelayCommand(
                async () =>
                {
                    string newFile = await BrowseForFile();
                    if (!string.IsNullOrEmpty(newFile))
                    {
                        await RefreshDiff(FileA, newFile);
                    }
                });

            SwapFiles = new AsyncRelayCommand(
                async () =>
                {
                    await RefreshDiff(FileB, FileA);
                });
        }

        private async Task<string> BrowseForFile()
        {
            FileOpenPicker picker = new FileOpenPicker();

            // https://github.com/microsoft/microsoft-ui-xaml/issues/4100
            IntPtr hwnd = _mainWindow.As<IWindowNative>().WindowHandle;
            IInitializeWithWindow initializeWithWindow = picker.As<IInitializeWithWindow>();
            initializeWithWindow.Initialize(hwnd);
            //

            picker.FileTypeFilter.Add("*");

            StorageFile file = await picker.PickSingleFileAsync();
            return file != null ? file.Path : "";
        }

        public async Task RefreshDiff(string fileA, string fileB)
        {
            FileA = fileA;
            FileB = fileB;

            FileAName = Path.GetFileName(FileA);
            FileBName = Path.GetFileName(FileB);

            if (!string.IsNullOrEmpty(FileA) && !string.IsNullOrEmpty(FileB))
            {
                await CurrentDiffViewModel.GenerateDiff.ExecuteAsync((FileA, FileB));
            }
        }
    }
}
