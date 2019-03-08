using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Linq;
using System.Text;
using TextEditor.Model;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DiffWit.Controls
{
    public sealed partial class TextControl : UserControl
    {
        private Windows.UI.Color _defaultBackgroundColor = Windows.UI.Color.FromArgb(255, 30, 30, 30);
        private Windows.UI.Color _defaultForegroundColor = Windows.UI.Color.FromArgb(255, 200, 200, 200);

        private Windows.UI.Color _addedBackgroundColor = Windows.UI.Color.FromArgb(255, 21, 53, 44);
        private Windows.UI.Color _removedBackgroundColor = Windows.UI.Color.FromArgb(255, 45, 0, 0);
        private Windows.UI.Color _nullBackgroundColor = Windows.UI.Color.FromArgb(255, 58, 58, 58);

        private Windows.UI.Color _addedForegroundColor = Windows.UI.Color.FromArgb(255, 60, 193, 160);
        private Windows.UI.Color _removedForegroundColor = Windows.UI.Color.FromArgb(255, 229, 78, 55);

        private CanvasSolidColorBrush _defaultForegroundBrush;
        private CanvasSolidColorBrush _addedForegroundBrush;
        private CanvasSolidColorBrush _removedForegroundBrush;
        private CanvasLinearGradientBrush _nullBrush;

        private float LineHeight = 14.05078125f;
        private const int WidthPadding = 8;

        private CanvasTextFormat _textFormat;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(ITextModel), typeof(TextControl),
                new PropertyMetadata(default(ITextModel), OnTextChanged));

        public ITextModel Text
        {
            get { return (ITextModel)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textView = (TextControl)d;
            textView.UpdateText();
        }

        public TextControl()
        {
            this.InitializeComponent();
            
            _textFormat = new CanvasTextFormat
            {
                FontFamily = "Consolas",
                FontSize = 12,
                WordWrapping = CanvasWordWrapping.NoWrap
            };

            CanvasRoot.CreateResources += (s, e) =>
            {
                _defaultForegroundBrush = new CanvasSolidColorBrush(s, _defaultForegroundColor);
                _addedForegroundBrush = new CanvasSolidColorBrush(s, _addedForegroundColor);
                _removedForegroundBrush = new CanvasSolidColorBrush(s, _removedForegroundColor);

                var stops = new CanvasGradientStop[4]
                {
                    new CanvasGradientStop { Color = _defaultBackgroundColor, Position = 0 },
                    new CanvasGradientStop { Color = _defaultBackgroundColor, Position = 0.5f },
                    new CanvasGradientStop { Color = _nullBackgroundColor, Position = 0.5f },
                    new CanvasGradientStop { Color = _nullBackgroundColor, Position = 1 }
                };

                _nullBrush = new CanvasLinearGradientBrush(s, stops, CanvasEdgeBehavior.Mirror, CanvasAlphaMode.Premultiplied)
                {
                    StartPoint = new System.Numerics.Vector2(0, 0),
                    EndPoint = new System.Numerics.Vector2(4, 4)
                };
            };

            CanvasRoot.SizeChanged += (s, e) =>
            {
                CanvasRoot.Invalidate();
            };
        }
        
        internal void UpdateText()
        {
            CanvasRoot.Height = Text.LineCount * LineHeight;
            CanvasRoot.Invalidate();

            GutterRoot.Text = this.Text;
            GutterRoot.UpdateHeight(CanvasRoot.Height);
        }

        private void canvas_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            if (Text == null)
                return;

            foreach (var region in args.InvalidatedRegions)
            {
                using (var ds = sender.CreateDrawingSession(region))
                {
                    ds.Clear(_defaultBackgroundColor);
                    
                    var startLine = (int)Math.Clamp(Math.Floor(region.Top / LineHeight), 0, Text.LineCount);

                    // add 2 to the end line count. we want to "overdraw" a bit if the line is going to be cut-off
                    var endLine = (int)Math.Clamp(Math.Ceiling(region.Bottom / LineHeight) + 2, 0, Text.LineCount);

                    var stringRegion = new StringBuilder();
                    for (int i = startLine; i < endLine; i++)
                    {
                        var line = Text.GetLine(i);
                        if (line is DiffTextLine diffLine)
                        {
                            if (diffLine.ChangeType == DiffLineType.Empty)
                            {
                                stringRegion.AppendLine(); // null line
                            }
                            else
                            {
                                stringRegion.AppendLine(Text.GetLine(i).ToString());
                            }
                        }
                    }

                    using (var canvasText = new CanvasTextLayout(ds, stringRegion.ToString(), _textFormat, 9999, (float)region.Height))
                    {
                        // Handle the diff line background colors
                        // TODO: use the ITextDecoration interface
                        int index = 0;
                        for (int i = startLine; i < endLine; i++)
                        {
                            var line = Text.GetLine(i);

                            if (line is DiffTextLine diffLine)
                            {
                                switch (diffLine.ChangeType)
                                {
                                    case DiffLineType.Insert:
                                        {
                                            ds.FillRectangle(0, i * LineHeight, (float)this.ActualWidth, MathF.Ceiling(LineHeight), _addedBackgroundColor);
                                            canvasText.SetBrush(index, line.Length, _addedForegroundBrush);
                                        }
                                        break;
                                    case DiffLineType.Remove:
                                        {
                                            ds.FillRectangle(0, i * LineHeight, (float)this.ActualWidth, MathF.Ceiling(LineHeight), _removedBackgroundColor);
                                            canvasText.SetBrush(index, line.Length, _removedForegroundBrush);
                                        }
                                        break;
                                    case DiffLineType.Empty:
                                        {
                                            ds.FillRectangle(0, i * LineHeight, (float)this.ActualWidth, MathF.Ceiling(LineHeight), _nullBrush);
                                        }
                                        break;
                                    case DiffLineType.Unchanged:
                                    default:
                                        break;
                                }
                            }

                            index += line.Length + 2; // add for newline
                        }
                        
                        // Draw the text
                        ds.DrawTextLayout(canvasText, 0, startLine * LineHeight, _defaultForegroundBrush);
                    }
                }
            }
        }
    }
}
