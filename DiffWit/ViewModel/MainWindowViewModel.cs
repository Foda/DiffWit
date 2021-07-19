using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiffWit.Utils;
using System;
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

        public bool HasValidDiff
        {
            get
            {
                return !string.IsNullOrEmpty(FileA) && !string.IsNullOrEmpty(FileB);
            }
        }

        public AsyncRelayCommand BrowseForFileA { get; }
        public AsyncRelayCommand BrowseForFileB { get; }

        private SplitDiffViewModel _splitDiffViewModel;
        private UnifiedDiffViewModel _unifiedDiffViewModel;

        public MainWindowViewModel(MainWindow parentWindow)
        {
            _mainWindow = parentWindow;

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

            _unifiedDiffViewModel = new UnifiedDiffViewModel(FileA, FileB);
            _splitDiffViewModel = new SplitDiffViewModel(FileA, FileB);

            CurrentDiffViewModel = _splitDiffViewModel;

            await CurrentDiffViewModel.GenerateDiff.ExecuteAsync(null);
        }
    }
}
