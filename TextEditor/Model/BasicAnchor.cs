using System;
using System.Collections.Generic;
using System.Text;

namespace TextEditor.Model
{
    public class BasicAnchor : IAnchorPos
    {
        public ITextLine AdornedLine { get; }

        public BasicAnchor(ITextLine line)
        {
            AdornedLine = line;
        }
    }
}
