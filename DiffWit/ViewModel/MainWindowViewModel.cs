using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffWit.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
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
            private set => SetProperty(ref _fileA, value);
        }

        private string _fileB;
        public string FileB
        {
            get => _fileB;
            private set => SetProperty(ref _fileB, value);
        }

        private SplitDiffViewModel _splitDiffViewModel;
        private UnifiedDiffViewModel _unifiedDiffViewModel;

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
