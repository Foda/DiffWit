using System;
using System.Collections.Generic;
using System.Text;

namespace TextEditor.Model
{
    public class TextLine : ITextLine
    {
        public IEnumerable<ITextRun> Runs { get; }

        public TextLine(string text)
        {
            Runs = new List<ITextRun>
            {
                new TextRun(text)
            };
        }

        public int Length
        {
            get
            {
                int length = 0;
                foreach (var run in Runs)
                {
                    length += run.Text.Length;
                }

                return length;
            }
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
