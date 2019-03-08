using System;
using System.Collections.Generic;
using System.Text;

namespace TextEditor.Model
{
    public interface ITextModel
    {
        int LineCount { get; }
        ITextLine GetLine(int index);

        int GetLineNo(ITextLine line);

        void InsertLine(ITextLine line);
        void InsertLine(int index, ITextLine line);
    }
}
