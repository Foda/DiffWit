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
    public class UnifiedDiffViewModel : BaseDiffViewModel
    {
        private TextModel _unifiedDiffTextModel;
        public TextModel UnifiedDiffTextModel
        {
            get { return _unifiedDiffTextModel; }
            private set { this.RaiseAndSetIfChanged(ref _unifiedDiffTextModel, value); }
        }

        public ReactiveCommand<Unit, Unit> ScrollToPreviousChange { get; }
        public ReactiveCommand<Unit, Unit> ScrollToNextChange { get; }

        public UnifiedDiffViewModel()
        {
            ScrollToPreviousChange = ReactiveCommand.Create(ScrollToPreviousChange_Impl);
            ScrollToNextChange = ReactiveCommand.Create(ScrollToNextChange_Impl);
        }

        internal override void ProcessDiff()
        {
            if (string.IsNullOrEmpty(FileA) || string.IsNullOrEmpty(FileB) || _diffCache == null)
                return;

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
