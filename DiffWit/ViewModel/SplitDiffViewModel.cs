using ColorCode;
using ColorCode.Styling;
using DiffWit.Utils;
using Microsoft.Toolkit.Uwp.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Diff;
using TextEditor.Model;
using TextEditor.Utils;
using Windows.Storage;

namespace DiffWit.ViewModel
{
    public class SplitDiffViewModel : ReactiveObject, IDiffViewModel
    {
        private List<Diff> _diffCache = new List<Diff>();
        private List<IAnchorPos> _diffAnchors = new List<IAnchorPos>();
        private int _currentChange = 0;
        
        public string FileA { get; }
        public string FileB { get; }

        public string FileExtension
        {
            get { return Path.GetExtension(FileA).Remove(0, 1); }
        }

        private TextModel _leftDiffTextModel;
        public TextModel LeftDiffTextModel
        {
            get { return _leftDiffTextModel; }
            private set { this.RaiseAndSetIfChanged(ref _leftDiffTextModel, value); }
        }

        private TextModel _rightDiffTextModel;
        public TextModel RightDiffTextModel
        {
            get { return _rightDiffTextModel; }
            private set { this.RaiseAndSetIfChanged(ref _rightDiffTextModel, value); }
        }

        public int ChangeCount { get { return _diffCache.Count; } }

        public ReactiveCommand<Unit, Unit> ScrollToPreviousChange { get; }
        public ReactiveCommand<Unit, Unit> ScrollToNextChange { get; }

        public SplitDiffViewModel(string fileA, string fileB, List<Diff> diffCache)
        {
            FileA = fileA;
            FileB = fileB;

            _diffCache = diffCache;

            ScrollToPreviousChange = ReactiveCommand.Create(ScrollToPreviousChange_Impl);
            ScrollToNextChange = ReactiveCommand.Create(ScrollToNextChange_Impl);
        }

        public void ProcessDiff()
        {
            var result = DiffFactory.GenerateSplitDiff(_diffCache);

            LeftDiffTextModel = result.SideA;
            RightDiffTextModel = result.SideB;
            _diffAnchors = result.DiffAnchors;
        }

        private void ScrollToPreviousChange_Impl()
        {
            _currentChange--;
            if (_currentChange < 0)
            {
                _currentChange = _diffAnchors.Count - 1;
            }

            var anchor = _diffAnchors[_currentChange];
            ScrollToAnchor(anchor);
        }

        private void ScrollToNextChange_Impl()
        {
            _currentChange++;
            if (_currentChange >= _diffAnchors.Count)
            {
                _currentChange = 0;
            }

            var anchor = _diffAnchors[_currentChange];
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
