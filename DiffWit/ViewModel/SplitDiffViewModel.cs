using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiffWit.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TextEditor.Diff;
using TextEditor.Model;
using TextEditor.Utils;
using Windows.Storage;

namespace DiffWit.ViewModel
{
    public class SplitDiffViewModel : ObservableObject, IDiffViewModel
    {
        private List<Diff> _diffCache = new();
        private List<IAnchorPos> _diffAnchors = new();
        private int _currentChange = 0;

        public string FileExtensionFileA { get; private set; }
        public string FileExtensionFileB { get; private set; }

        private TextModel _leftDiffTextModel;
        public TextModel LeftDiffTextModel
        {
            get => _leftDiffTextModel;
            private set => SetProperty(ref _leftDiffTextModel, value);
        }

        private TextModel _rightDiffTextModel;
        public TextModel RightDiffTextModel
        {
            get => _rightDiffTextModel;
            private set => SetProperty(ref _rightDiffTextModel, value);
        }

        private int _changeCount;
        public int ChangeCount
        {
            get => _changeCount;
            private set => SetProperty(ref _changeCount, value);
        }

        public RelayCommand ScrollToPreviousChange { get; }
        public RelayCommand ScrollToNextChange { get; }

        public AsyncRelayCommand<(string fileA, string fileB)> GenerateDiff { get; }

        public SplitDiffViewModel()
        {
            ScrollToPreviousChange = new RelayCommand(ScrollToPreviousChange_Impl);
            ScrollToNextChange = new RelayCommand(ScrollToNextChange_Impl);

            GenerateDiff = new AsyncRelayCommand<(string fileA, string fileB)>(async (files) =>
            {
                FileExtensionFileA = Path.GetExtension(files.fileA).Remove(0, 1);
                FileExtensionFileB = Path.GetExtension(files.fileB).Remove(0, 1);

                List<Diff> diff = await DiffCacheUtil.GenerateDiffCache(files.fileA, files.fileB);

                SplitDiffModel diffModel = DiffFactory.GenerateSplitDiff(diff);

                ChangeCount = diff.Count;
                LeftDiffTextModel = diffModel.SideA;
                RightDiffTextModel = diffModel.SideB;
                _diffAnchors = diffModel.DiffAnchors;
            });
        }

        private void ScrollToPreviousChange_Impl()
        {
            _currentChange--;
            if (_currentChange < 0)
            {
                _currentChange = _diffAnchors.Count - 1;
            }

            IAnchorPos anchor = _diffAnchors[_currentChange];
            ScrollToAnchor(anchor);
        }

        private void ScrollToNextChange_Impl()
        {
            _currentChange++;
            if (_currentChange >= _diffAnchors.Count)
            {
                _currentChange = 0;
            }

            IAnchorPos anchor = _diffAnchors[_currentChange];
            ScrollToAnchor(anchor);
        }

        private void ScrollToAnchor(IAnchorPos anchor)
        {
            if (anchor.AdornedLine is DiffTextLine diffLine)
            {
                if (diffLine.ChangeType == DiffLineType.Remove)
                {
                    LeftDiffTextModel.ScrollToAnchor(anchor);
                }
                else if (diffLine.ChangeType == DiffLineType.Insert)
                {
                    RightDiffTextModel.ScrollToAnchor(anchor);
                }
            }
        }
    }
}
