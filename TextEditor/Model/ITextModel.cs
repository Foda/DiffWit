using System;
using System.Collections.Generic;
using System.Text;

namespace TextEditor.Model
{
    public interface ITextModel
    {
        event EventHandler<ITextLine> ScrollToLineRequested;

        int LineCount { get; }
        int ValidLineCount { get; }
        ITextLine GetLine(int index);

        int GetLineNo(ITextLine line);

        void InsertLine(ITextLine line);
        void InsertLine(int index, ITextLine line);

        IAnchorPos CreateAnchor(ITextLine line);

        void ScrollToAnchor(IAnchorPos anchor);
    }
}
