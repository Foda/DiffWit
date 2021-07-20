using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiffWit.Utils;
using System.Collections.Generic;
using TextEditor.Diff;
using TextEditor.Model;

namespace DiffWit.ViewModel
{
    public class UnifiedDiffViewModel : ObservableObject, IDiffViewModel
    {
        private TextModel _unifiedDiffTextModel;
        public TextModel UnifiedDiffTextModel
        {
            get { return _unifiedDiffTextModel; }
            private set { SetProperty(ref _unifiedDiffTextModel, value); }
        }

        private int _changeCount;
        public int ChangeCount
        {
            get { return _changeCount; }
            private set { SetProperty(ref _changeCount, value); }
        }

        public RelayCommand ScrollToPreviousChange { get; }
        public RelayCommand ScrollToNextChange { get; }

        public AsyncRelayCommand<(string fileA, string fileB)> GenerateDiff { get; }

        public UnifiedDiffViewModel()
        {
            ScrollToPreviousChange = new RelayCommand(ScrollToPreviousChange_Impl);
            ScrollToNextChange = new RelayCommand(ScrollToNextChange_Impl);

            GenerateDiff = new AsyncRelayCommand<(string fileA, string fileB)>(async (files) =>
            {
                List<Diff> diff = await DiffCacheUtil.GenerateDiffCache(files.fileA, files.fileB);

                ChangeCount = diff.Count;
                UnifiedDiffTextModel = DiffFactory.GenerateUnifiedDiff(diff);
            });
        }

        private void ScrollToPreviousChange_Impl()
        {

        }

        private void ScrollToNextChange_Impl()
        {

        }
    }
}
