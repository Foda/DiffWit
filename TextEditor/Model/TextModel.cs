using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextEditor.Model
{
    public abstract class TextModel : ITextModel
    {
        internal List<ITextLine> _lines = new List<ITextLine>();
        protected List<ITextLine> Lines => _lines;

        public int LineCount => _lines.Count;

        public ITextLine GetLine(int index)
        {
            return _lines[index];
        }

        public virtual int GetLineNo(ITextLine line)
        {
            return _lines.IndexOf(line);
        }

        public TextModel(string text)
        {
            // split by new line
            Lines.AddRange(
                text.Replace("\r\n", "\n")
                    .Split('\n')
                    .Select(line => new TextLine(line)));
        }

        public TextModel() { }

        public void InsertLine(ITextLine line)
        {
            Lines.Add(line);
        }

        public void InsertLine(int index, ITextLine line)
        {
            Lines.Insert(index, line);
        }
        
        public override string ToString()
        {
            var newLine = "\r\n";

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < LineCount; i++)
            {
                stringBuilder.Append(GetLine(i).ToString());
                if (i + 1 < LineCount)
                {
                    stringBuilder.Append(newLine);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
