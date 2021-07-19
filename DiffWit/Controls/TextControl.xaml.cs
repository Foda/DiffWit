using ColorCode;
using ColorCode.Styling;
using DiffWit.Utils;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TextEditor.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DiffWit.Controls
{
    public sealed partial class TextControl : UserControl
    {
        private DiffViewTextColorizer _colorizer = new DiffViewTextColorizer(StyleDictionary.DefaultDark);
        private ILanguage _language;

        private Windows.UI.Color _defaultBackgroundColor = Windows.UI.Color.FromArgb(255, 30, 30, 30);
        private Windows.UI.Color _defaultForegroundColor = Windows.UI.Color.FromArgb(255, 245, 245, 245);

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
        private ScrollViewer _parentScrollViewer;

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

        public static readonly DependencyProperty DiffMapProperty =
            DependencyProperty.Register(nameof(DiffMap), typeof(DiffMapScrollControl), typeof(TextControl),
                new PropertyMetadata(default(DiffMapScrollControl)));

        public DiffMapScrollControl DiffMap
        {
            get { return (DiffMapScrollControl)GetValue(DiffMapProperty); }
            set { SetValue(DiffMapProperty, value); }
        }

        public static readonly DependencyProperty FileExtensionProperty =
            DependencyProperty.Register(nameof(FileExtension), typeof(string), typeof(TextControl), new PropertyMetadata(""));

        public string FileExtension
        {
            get { return (string)GetValue(FileExtensionProperty); }
            set { SetValue(FileExtensionProperty, value); }
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

        public int VisibleTopLine
        {
            get
            {
                if (_parentScrollViewer == null)
                    return 0;

                int startLine = (int)(_parentScrollViewer.VerticalOffset / LineHeight);
                int endLine = (int)Math.Min(
                    (_parentScrollViewer.VerticalOffset + _parentScrollViewer.ViewportHeight) / LineHeight, Text.LineCount - 1);


                for (int i = startLine; i < endLine; i++)
                {
                    if (Text.GetLine(i) is DiffTextLine diffText)
                    {
                        if (diffText.ChangeType != DiffLineType.Empty)
                        {
                            return diffText.LineNo;
                        }
                    }
                }

                return 0;
            }
        }

        public int VisibleBottomLine
        {
            get
            {
                if (_parentScrollViewer == null)
                {
                    return 0;
                }

                int startLine = (int)(_parentScrollViewer.VerticalOffset / LineHeight);
                int endLine = (int)Math.Min(
                    (_parentScrollViewer.VerticalOffset + _parentScrollViewer.ViewportHeight) / LineHeight, Text.LineCount - 1);

                for (int i = endLine; i > startLine; i--)
                {
                    if (Text.GetLine(i) is DiffTextLine diffText)
                    {
                        if (diffText.ChangeType != DiffLineType.Empty)
                            return i;
                    }
                }

                return 0;
            }
        }

        internal void UpdateText()
        {
            if (Text == null)
            {
                return;
            }

            CanvasRoot.Height = Text.LineCount * LineHeight;
            CanvasRoot.Invalidate();

            GutterRoot.Text = this.Text;
            GutterRoot.UpdateHeight(CanvasRoot.Height);

            if (DiffMap != null)
            {
                DiffMap.UpdateTextViews();
            }

            Text.ScrollToLineRequested -= Text_ScrollToLineRequested;
            Text.ScrollToLineRequested += Text_ScrollToLineRequested;
        }

        private void Text_ScrollToLineRequested(object sender, ITextLine line)
        {
            if (_parentScrollViewer == null)
            {
                return;
            }

            var lineIndex = Text.GetLineNo(line);
            var verticalOffset = lineIndex * LineHeight;

            _parentScrollViewer.ChangeView(null, verticalOffset, null);
        }

        private void canvas_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            if (Text == null)
            {
                return;
            }

            // Update the diff map
            if (_parentScrollViewer == null)
            {
                _parentScrollViewer = VisualTreeHelper.FindParent<ScrollViewer>(this);
                if (_parentScrollViewer != null)
                {
                    _parentScrollViewer.ViewChanged += (s, e) =>
                    {
                        if (DiffMap != null)
                        {
                            DiffMap.UpdateTextViews();
                        }
                    };

                    // Force update once
                    if (DiffMap != null)
                    {
                        DiffMap.UpdateTextViews();
                    }
                }
            }

            if (_language == null && !string.IsNullOrEmpty(FileExtension))
            {
                _language = Languages.FindById(FileExtension);
            }

            foreach (Windows.Foundation.Rect region in args.InvalidatedRegions)
            {
                using (CanvasDrawingSession ds = sender.CreateDrawingSession(region))
                {
                    ds.Clear(_defaultBackgroundColor);

                    int startLine = (int)Math.Clamp(Math.Floor(region.Top / LineHeight), 0, Text.LineCount);

                    // add 2 to the end line count. we want to "overdraw" a bit if the line is going to be cut-off
                    int endLine = (int)Math.Clamp(Math.Ceiling(region.Bottom / LineHeight) + 2, 0, Text.LineCount);

                    StringBuilder stringRegion = new StringBuilder();
                    for (int i = startLine; i < endLine; i++)
                    {
                        ITextLine line = Text.GetLine(i);
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
                                            if (_language == null)
                                            {
                                                canvasText.SetBrush(index, line.Length, _addedForegroundBrush);
                                            }
                                        }
                                        break;
                                    case DiffLineType.Remove:
                                        {
                                            ds.FillRectangle(0, i * LineHeight, (float)this.ActualWidth, MathF.Ceiling(LineHeight), _removedBackgroundColor);
                                            if (_language == null)
                                            {
                                                canvasText.SetBrush(index, line.Length, _removedForegroundBrush);
                                            }
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

                        // Syntax highlight
                        if (_language != null)
                        {
                            _colorizer.FormatLine(_language, CanvasRoot, canvasText, stringRegion.ToString());
                        }

                        // Draw the text
                        ds.DrawTextLayout(canvasText, 0, startLine * LineHeight, _defaultForegroundBrush);
                    }
                }
            }
        }
    }
}
