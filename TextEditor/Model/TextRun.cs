using System;
using System.Collections.Generic;
using System.Text;

namespace TextEditor.Model
{
    public class TextRun : ITextRun
    {
        public string Text { get; }

        public ITextDecoration Decoration { get; }

        public TextRun(string text)
        {
            Text = text;
        }
    }
}
