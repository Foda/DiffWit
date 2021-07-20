using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Linq;
using System.Text;
using TextEditor.Model;
using Windows.Foundation;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DiffWit.Controls
{
    public sealed partial class GutterControl : UserControl
    {
        private Windows.UI.Color _defaultForegroundColor = Windows.UI.Color.FromArgb(255, 43, 145, 175);
        
        private CanvasSolidColorBrush _defaultForegroundBrush;

        private CanvasTextFormat _textFormat;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(ITextModel), typeof(GutterControl),
                new PropertyMetadata(default(ITextModel), OnTextChanged));

        public ITextModel Text
        {
            get { return (ITextModel)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textView = (GutterControl)d;
            textView.UpdateText();
        }

        private float LineHeight = 14.05078125f;

        public GutterControl()
        {
            this.InitializeComponent();
            
            _textFormat = new CanvasTextFormat
            {
                FontFamily = "Consolas",
                FontSize = 12,
                WordWrapping = CanvasWordWrapping.NoWrap,
                HorizontalAlignment = CanvasHorizontalAlignment.Right
            };

            CanvasRoot.CreateResources += (s, e) =>
            {
                _defaultForegroundBrush = new CanvasSolidColorBrush(s, _defaultForegroundColor);
            };

            CanvasRoot.SizeChanged += (s, e) =>
            {
                CanvasRoot.Invalidate();
            };
        }

        internal void UpdateText()
        {
            CanvasRoot.Invalidate();
        }

        public void UpdateHeight(double height)
        {
            CanvasRoot.Height = height;
            CanvasRoot.Invalidate();
        }
        
        private void canvas_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            foreach (var region in args.InvalidatedRegions)
            {
                using (var ds = sender.CreateDrawingSession(region))
                {
                    ds.Clear(Colors.Transparent);
                    
                    int startLine = (int)Math.Floor(region.Top / LineHeight);

                    // add 2 to the end line count. we want to "overdraw" a bit if the line is going to be cut-off
                    int endLine = (int)(Math.Ceiling(region.Bottom / LineHeight) + 2); 

                    var stringRegion = new StringBuilder();
                    for (int i = startLine; i < endLine; i++)
                    {
                        if (Text != null && i < Text.LineCount)
                        {
                            var textLine = Text.GetLine(i);
                            if (textLine != null && textLine is DiffTextLine diffLine)
                            {
                                stringRegion.AppendLine(diffLine.LineNo > -1 ? (diffLine.LineNo).ToString() : "");
                            }
                        }
                    }
                    
                    using (var canvasText = new CanvasTextLayout(ds, stringRegion.ToString(), _textFormat, 32, (float)region.Height))
                    {
                        ds.DrawTextLayout(canvasText, 0, startLine * LineHeight, _defaultForegroundBrush);
                    }
                }
            }
        }
    }
}
