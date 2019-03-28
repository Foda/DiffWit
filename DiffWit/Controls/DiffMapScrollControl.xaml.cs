﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using TextEditor.Model;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DiffWit.Controls
{
    public sealed partial class DiffMapScrollControl : UserControl
    {
        private Windows.UI.Color _fileSizeColor = Windows.UI.Color.FromArgb(255, 37, 41, 37);
        private Windows.UI.Color _lassoColor = Windows.UI.Color.FromArgb(255, 140, 140, 140);

        private Windows.UI.Color _addedBackgroundColor = Windows.UI.Color.FromArgb(255, 60, 193, 160);
        private Windows.UI.Color _removedBackgroundColor = Windows.UI.Color.FromArgb(255, 229, 78, 55);

        private double _leftFileTop = 0;
        private double _rightFileTop = 0;
        private double _leftFileHeight = 0;
        private double _rightFileHeight = 0;

        public static readonly DependencyProperty LeftTextViewProperty =
            DependencyProperty.Register(nameof(LeftTextView), typeof(TextControl), typeof(DiffMapScrollControl),
                new PropertyMetadata(null, OnTextViewChanged));

        public TextControl LeftTextView
        {
            get { return (TextControl)GetValue(LeftTextViewProperty); }
            set
            {
                SetValue(LeftTextViewProperty, value);
                if (value != null)
                {
                    value.DiffMap = this;
                }
            }
        }

        public static readonly DependencyProperty RightTextViewProperty =
            DependencyProperty.Register(nameof(RightTextView), typeof(TextControl), typeof(DiffMapScrollControl),
                new PropertyMetadata(null, OnTextViewChanged));

        public TextControl RightTextView
        {
            get { return (TextControl)GetValue(RightTextViewProperty); }
            set
            {
                SetValue(RightTextViewProperty, value);
                if (value != null)
                {
                    value.DiffMap = this;
                }
            }
        }

        public double RealHeight
        {
            get { return ActualHeight - 20; }
        }

        private static void OnTextViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var diffScrollView = (DiffMapScrollControl)d;
            diffScrollView.UpdateTextViews();
        }

        public DiffMapScrollControl()
        {
            this.InitializeComponent();

            CanvasRoot.SizeChanged += (s, e) =>
            {
                CanvasRoot.Invalidate();
            };
        }

        public void UpdateTextViews()
        {
            if (LeftTextView != null && RightTextView != null)
            {
                UpdateFileSizes();
                CanvasRoot.Invalidate();
            }
        }

        private void UpdateFileSizes()
        {
            int lineCountOrg = LeftTextView.Text.ValidLineCount;
            int lineCountCur = RightTextView.Text.ValidLineCount;
            int maxLineCount = Math.Max(lineCountOrg, lineCountCur);

            double leftHeight = 0;
            double rightHeight = 0;

            if (lineCountOrg != lineCountCur)
            {
                if (lineCountOrg == maxLineCount)
                {
                    rightHeight += (double)(lineCountOrg - lineCountCur) / maxLineCount * RealHeight / 2.0;
                }
                else
                {
                    leftHeight += (double)(lineCountCur - lineCountOrg) / maxLineCount * RealHeight / 2.0;
                }
            }

            double leftOffset = (double)lineCountOrg / maxLineCount * RealHeight;
            double rightOffset = (double)lineCountCur / maxLineCount * RealHeight;

            _leftFileTop = Math.Floor(leftHeight);
            _rightFileTop = Math.Floor(rightHeight);
            _leftFileHeight = Math.Ceiling(leftHeight + leftOffset) - _leftFileTop;
            _rightFileHeight = Math.Ceiling(rightHeight + rightOffset) - _rightFileTop;
        }

        private void DrawOutlineChanges(CanvasDrawingSession ds)
        {
            int lineCountOrg = LeftTextView.Text.ValidLineCount;
            int lineCountCur = RightTextView.Text.ValidLineCount;
            int maxLineCount = Math.Max(lineCountOrg, lineCountCur);

            int singleOrgLineHeight = (int)(RealHeight / lineCountOrg);
            int singleCurLineHeight = (int)(RealHeight / lineCountCur);

            // Draw removals
            for (int i = 0; i < LeftTextView.Text.LineCount; i++)
            {
                var textLine = LeftTextView.Text.GetLine(i);
                if (textLine is DiffTextLine diffLine)
                {
                    if (diffLine.ChangeType == DiffLineType.Remove)
                    {
                        int ypos = (int)((double)diffLine.LineNo / maxLineCount * RealHeight);
                        ds.FillRectangle(new Rect(12.0, ypos + (int)_leftFileTop, 16.0, singleOrgLineHeight + 1),
                            _removedBackgroundColor);
                    }
                }
            }

            for (int i = 0; i < RightTextView.Text.LineCount; i++)
            {
                var textLine = RightTextView.Text.GetLine(i);

                if (textLine is DiffTextLine diffLine)
                {
                    if (diffLine.ChangeType == DiffLineType.Insert)
                    {
                        int ypos = (int)((double)diffLine.LineNo / maxLineCount * RealHeight);
                        ds.FillRectangle(new Rect(40.0, ypos + (int)_rightFileTop, 16.0, singleCurLineHeight + 1),
                            _addedBackgroundColor);
                    }
                }
            }
        }

        private void DrawFileViewLasso(
            CanvasDrawingSession ds, float leftTop, float leftHeight, float rightTop, float rightHeight)
        {
            var pathBuilder = new CanvasPathBuilder(ds);

            float num1 = Math.Min(4.0f, leftHeight * 0.5f);
            float num2 = Math.Min(4.0f, rightHeight * 0.5f);
            float y1 = leftTop + leftHeight;
            float y2 = rightTop + rightHeight;

            pathBuilder.BeginFigure(new Vector2(32.0f, leftTop));
            pathBuilder.AddLine(new Vector2(36.0f, rightTop));
            pathBuilder.AddLine(new Vector2(56.0f, rightTop));
            pathBuilder.AddQuadraticBezier(new Vector2(60.0f, rightTop), new Vector2(60.0f, rightTop + num2));
            pathBuilder.AddLine(new Vector2(60.0f, y2 - num2));
            pathBuilder.AddQuadraticBezier(new Vector2(60.0f, y2), new Vector2(56.0f, y2));
            pathBuilder.AddLine(new Vector2(36.0f, y2));
            pathBuilder.AddLine(new Vector2(32.0f, y1));
            pathBuilder.AddLine(new Vector2(12.0f, y1));
            pathBuilder.AddQuadraticBezier(new Vector2(8.0f, y1), new Vector2(8.0f, y1 - num1));
            pathBuilder.AddLine(new Vector2(8.0f, leftTop + num1));
            pathBuilder.AddQuadraticBezier(new Vector2(8.0f, leftTop), new Vector2(12.0f, leftTop));
            pathBuilder.EndFigure(CanvasFigureLoop.Closed);

            var finalGeo = CanvasGeometry.CreatePath(pathBuilder);
            ds.DrawGeometry(finalGeo, _lassoColor);
        }

        private void CanvasRoot_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            using (var ds = args.DrawingSession)
            {
                if (LeftTextView != null && RightTextView != null)
                {
                    ds.Clear(Colors.Transparent);

                    var stroke = new CanvasStrokeStyle();

                    // draw file sizes for left + right
                    ds.FillRectangle(new Rect(12.0, _leftFileTop, 16.0, _leftFileHeight), _fileSizeColor);
                    ds.FillRectangle(new Rect(40.0, _rightFileTop, 16.0, _rightFileHeight), _fileSizeColor);

                    // draw changes
                    DrawOutlineChanges(ds);

                    var maxLineCount = Math.Max(LeftTextView.Text.ValidLineCount, RightTextView.Text.ValidLineCount);

                    //Left side
                    var leftTop = (float)Math.Max(Math.Floor((float)LeftTextView.VisibleTopLine / maxLineCount * RealHeight + _leftFileTop), _leftFileTop);
                    var leftBottomOffset = (float)Math.Min(Math.Ceiling((float)LeftTextView.VisibleBottomLine / maxLineCount * RealHeight + _leftFileTop), _leftFileTop + _leftFileHeight);
                    // 
                    
                    //Right side
                    var rightTop = (float)Math.Max(Math.Floor((float)RightTextView.VisibleTopLine / maxLineCount * RealHeight + _rightFileTop), _rightFileTop);
                    var rightBottomOffset = (float)Math.Min(Math.Ceiling((float)RightTextView.VisibleBottomLine / maxLineCount * RealHeight + _rightFileTop), _rightFileTop + _rightFileHeight);

                    DrawFileViewLasso(ds, leftTop, leftBottomOffset - leftTop, rightTop, rightBottomOffset - rightTop);
                }
            }
        }
    }
}