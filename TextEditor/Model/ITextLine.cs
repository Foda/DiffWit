using System;
using System.Collections.Generic;
using System.Text;

namespace TextEditor.Model
{
    public interface ITextLine
    {
        IEnumerable<ITextRun> Runs { get; }
        int Length { get; }
    }
}
