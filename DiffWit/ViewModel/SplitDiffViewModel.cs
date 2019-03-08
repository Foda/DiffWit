using Microsoft.Toolkit.Uwp.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
        public string FileA { get; }
        public string FileB { get; }

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

        public SplitDiffViewModel(string fileA, string fileB, List<Diff> diffCache)
        {
            FileA = fileA;
            FileB = fileB;
            _diffCache = diffCache;
        }

        public void ProcessDiff()
        {
            var result = DiffFactory.GenerateSplitDiff(_diffCache);

            LeftDiffTextModel = result.Item1;
            RightDiffTextModel = result.Item2;
        }
    }
}
