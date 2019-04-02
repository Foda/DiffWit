using ColorCode;
using ColorCode.Parsing;
using ColorCode.Styling;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;


namespace DiffWit.Utils
{
    public class DiffViewTextColorizer : CodeColorizerBase
    {
        private Dictionary<Color, CanvasSolidColorBrush> _brushLookup = new Dictionary<Color, CanvasSolidColorBrush>();

        public DiffViewTextColorizer(StyleDictionary Style = null, ILanguageParser languageParser = null)
            : base(Style, languageParser)
        {

        }

        public void FormatLine(ILanguage language,
            CanvasVirtualControl canvas, CanvasTextLayout canvasText, string content)
        {
            var offset = 0;
            base.languageParser.Parse(content, language,
                    (parsedCode, captures) =>
                    {
                        foreach (var capture in captures)
                        {
                            if (base.Styles.Contains(capture.Name))
                            {
                                var style = base.Styles[capture.Name];

                                if (!string.IsNullOrEmpty(style.Foreground))
                                {
                                    var foregroundBrush = GetSolidColorBrush(canvas, style.Foreground);
                                    canvasText.SetBrush(
                                        capture.Index + offset,
                                        capture.Length, foregroundBrush);
                                }
                            }
                        }

                        offset += parsedCode.Length;
                    });
        }

        protected override void Write(string parsedSourceCode, IList<Scope> scopes)
        {
            // ???
        }

        private CanvasSolidColorBrush GetSolidColorBrush(CanvasVirtualControl resourceCreator, string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = 255;
            int index3 = 0;

            if (hex.Length == 8)
            {
                a = (byte)Convert.ToUInt32(hex.Substring(index3, 2), 16);
                index3 += 2;
            }

            byte r = (byte)Convert.ToUInt32(hex.Substring(index3, 2), 16);
            index3 += 2;

            byte g = (byte)Convert.ToUInt32(hex.Substring(index3, 2), 16);
            index3 += 2;

            byte b = (byte)Convert.ToUInt32(hex.Substring(index3, 2), 16);

            var color = Color.FromArgb(a, r, g, b);
            if (_brushLookup.TryGetValue(color, out CanvasSolidColorBrush brush))
            {
                return brush;
            }

            var newBrush = new CanvasSolidColorBrush(resourceCreator, color);
            _brushLookup.Add(color, newBrush);

            return newBrush;
        }
    }
}
