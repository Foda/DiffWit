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
    public class UnifiedDiffViewModel : ReactiveObject, IDiffViewModel
    {
        private List<Diff> _diffCache = new List<Diff>();

        public string FileA { get; }
        public string FileB { get; }

        private TextModel _unifiedDiffTextModel;
        public TextModel UnifiedDiffTextModel
        {
            get { return _unifiedDiffTextModel; }
            private set { this.RaiseAndSetIfChanged(ref _unifiedDiffTextModel, value); }
        }

        public int ChangeCount { get { return _diffCache.Count; } }

        public ReactiveCommand<Unit, Unit> ScrollToPreviousChange { get; }
        public ReactiveCommand<Unit, Unit> ScrollToNextChange { get; }

        public UnifiedDiffViewModel(string fileA, string fileB, List<Diff> diffCache)
        {
            FileA = fileA;
            FileB = fileB;
            _diffCache = diffCache;


            ScrollToPreviousChange = ReactiveCommand.Create(ScrollToPreviousChange_Impl);
            ScrollToNextChange = ReactiveCommand.Create(ScrollToNextChange_Impl);
        }

        public void ProcessDiff()
        {
            UnifiedDiffTextModel = DiffFactory.GenerateUnifiedDiff(_diffCache);
        }

        private void ScrollToPreviousChange_Impl()
        {

        }

        private void ScrollToNextChange_Impl()
        {

        }
    }
}
