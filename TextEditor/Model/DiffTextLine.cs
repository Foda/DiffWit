using System;
using System.Collections.Generic;
using System.Text;

namespace TextEditor.Model
{
    public enum DiffLineType
    {
        Unchanged,
        Insert,
        Remove,
        Empty
    }

    public class DiffTextLine : ITextLine
    {
        public IEnumerable<ITextRun> Runs { get; }

        public DiffLineType ChangeType { get; }

        public int Length { get; }

        public DiffTextLine(string text, DiffLineType changeType)
        {
            Runs = new List<ITextRun>
            {
                new TextRun(text)
            };
            
            ChangeType = changeType;

            foreach (var run in Runs)
            {
                Length += run.Text.Length;
            }
        }

        private int _lineNo = -1;
        public int LineNo
        {
            get
            {
                if (ChangeType == DiffLineType.Empty)
                    return -1;

                return _lineNo;
            }
            set { _lineNo = value; }
        }
        
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var run in Runs)
            {
                stringBuilder.Append(run.Text);
            }

            return stringBuilder.ToString();
        }
    }
}
