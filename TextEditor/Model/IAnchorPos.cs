using System;
using System.Collections.Generic;
using System.Text;

namespace TextEditor.Model
{
    public interface IAnchorPos
    {
        ITextLine AdornedLine { get; }
    }
}
