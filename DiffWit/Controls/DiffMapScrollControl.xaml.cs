using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Model;
using Windows.Foundation;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DiffWit.Controls
{
    public sealed partial class DiffMapScrollControl : UserControl
    {
        private Windows.UI.Color _fileSizeColor = Windows.UI.Color.FromArgb(255, 37, 41, 37);
        private Windows.UI.Color _lassoColor = Windows.UI.Color.FromArgb(255, 165, 165, 165);

        private Windows.UI.Color _addedBackgroundColor = Windows.UI.Color.FromArgb(255, 60, 193, 160);
        private Windows.UI.Color _removedBackgroundColor = Windows.UI.Color.FromArgb(255, 229, 78, 55);

        private double _leftFileTop = 0;
        private double _rightFileTop = 0;
        private double _leftFileHeight = 0;
        private double _rightFileHeight = 0;

        private CanvasCachedGeometry _outlineChangesAdded;
        private CanvasCachedGeometry _outlineChangesRemoved;

        private bool _requestedOutlineChangesRefresh = false;
        private Task _loadOutlineChangesTask;

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
            get { return ActualHeight; }
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
                _requestedOutlineChangesRefresh = true;
                CanvasRoot.Invalidate();
            }
        }

        private void UpdateFileSizes(ITextModel leftControl, ITextModel rightControl, double controlHeight)
        {
            if (leftControl == null || rightControl == null)
            {
                return;
            }

            int lineCountOrg = leftControl.ValidLineCount;
            int lineCountCur = rightControl.ValidLineCount;
            int maxLineCount = Math.Max(lineCountOrg, lineCountCur);

            double leftHeight = 0;
            double rightHeight = 0;

            if (lineCountOrg != lineCountCur)
            {
                if (lineCountOrg == maxLineCount)
                {
                    rightHeight += (double)(lineCountOrg - lineCountCur) / maxLineCount * controlHeight / 2.0;
                }
                else
                {
                    leftHeight += (double)(lineCountCur - lineCountOrg) / maxLineCount * controlHeight / 2.0;
                }
            }

            double leftOffset = (double)lineCountOrg / maxLineCount * controlHeight;
            double rightOffset = (double)lineCountCur / maxLineCount * controlHeight;

            _leftFileTop = Math.Floor(leftHeight);
            _rightFileTop = Math.Floor(rightHeight);
            _leftFileHeight = Math.Ceiling(leftHeight + leftOffset) - _leftFileTop;
            _rightFileHeight = Math.Ceiling(rightHeight + rightOffset) - _rightFileTop;
        }

        private void UpdateOutlineChanges(ICanvasResourceCreator canvas,
            ITextModel leftControl, ITextModel rightControl, double controlHeight)
        {
            if (leftControl == null || rightControl == null)
            {
                return;
            }

            int lineCountOrg = leftControl.ValidLineCount;
            int lineCountCur = rightControl.ValidLineCount;
            int maxLineCount = Math.Max(lineCountOrg, lineCountCur);

            int singleOrgLineHeight = (int)(controlHeight / lineCountOrg);
            int singleCurLineHeight = (int)(controlHeight / lineCountCur);

            var removedGeo = new List<CanvasGeometry>();
            var addedGeo = new List<CanvasGeometry>();

            // Draw removals
            for (int i = 0; i < leftControl.LineCount; i++)
            {
                var textLine = leftControl.GetLine(i);
                if (textLine is DiffTextLine diffLine)
                {
                    if (diffLine.ChangeType == DiffLineType.Remove)
                    {
                        int ypos = (int)((double)diffLine.LineNo / maxLineCount * controlHeight);

                        removedGeo.Add(CanvasGeometry.CreateRectangle(canvas,
                            new Rect(12.0, ypos + (int)_leftFileTop, 16.0, singleOrgLineHeight + 1)));
                    }
                }
            }

            for (int i = 0; i < rightControl.LineCount; i++)
            {
                var textLine = rightControl.GetLine(i);

                if (textLine is DiffTextLine diffLine)
                {
                    if (diffLine.ChangeType == DiffLineType.Insert)
                    {
                        int ypos = (int)((double)diffLine.LineNo / maxLineCount * controlHeight);

                        addedGeo.Add(CanvasGeometry.CreateRectangle(canvas,
                            new Rect(40.0, ypos + (int)_rightFileTop, 16.0, singleCurLineHeight + 1)));
                    }
                }
            }

            // Save the results into a cached geo
            var groupedRemovedGeo = CanvasGeometry.CreateGroup(canvas, removedGeo.ToArray());
            var groupedAddedGeo = CanvasGeometry.CreateGroup(canvas, addedGeo.ToArray());

            _outlineChangesRemoved = CanvasCachedGeometry.CreateFill(groupedRemovedGeo);
            _outlineChangesAdded = CanvasCachedGeometry.CreateFill(groupedAddedGeo);
        }

        private void CanvasRoot_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            using (var ds = args.DrawingSession)
            {
                if (LeftTextView != null && RightTextView != null &&
                    LeftTextView.Text != null && RightTextView.Text != null)
                {
                    UpdateFileSizes(LeftTextView.Text, RightTextView.Text, ActualHeight);
                    UpdateOutlineChanges(ds, LeftTextView.Text, RightTextView.Text, ActualHeight);

                    ds.Clear(Colors.Transparent);

                    // Draw file sizes
                    ds.FillRectangle(new Rect(12.0, _leftFileTop, 16.0, _leftFileHeight), _fileSizeColor);
                    ds.FillRectangle(new Rect(40.0, _rightFileTop, 16.0, _rightFileHeight), _fileSizeColor);

                    // Draw changes
                    if (_outlineChangesRemoved != null && _outlineChangesAdded != null)
                    {
                        ds.DrawCachedGeometry(_outlineChangesRemoved, _removedBackgroundColor);
                        ds.DrawCachedGeometry(_outlineChangesAdded, _addedBackgroundColor);
                    }

                    var maxLineCount = Math.Max(LeftTextView.Text.ValidLineCount, RightTextView.Text.ValidLineCount);

                    // Left side
                    var leftTop = (float)Math.Max(Math.Floor((float)LeftTextView.VisibleTopLine / maxLineCount * RealHeight + _leftFileTop), _leftFileTop);

                    var leftBottomOffset = (float)Math.Min(
                        Math.Ceiling((float)LeftTextView.VisibleBottomLine / maxLineCount * RealHeight + _leftFileTop), 
                        _leftFileTop + _leftFileHeight);
                    
                    // Right side
                    var rightTop = (float)Math.Max(Math.Floor((float)RightTextView.VisibleTopLine / maxLineCount * RealHeight + _rightFileTop), _rightFileTop);

                    var rightBottomOffset = (float)Math.Min(
                        Math.Ceiling((float)RightTextView.VisibleBottomLine / maxLineCount * RealHeight + _rightFileTop), 
                        _rightFileTop + _rightFileHeight);

                    // Visible area outline
                    DrawFileViewLasso(ds, leftTop, leftBottomOffset - leftTop, rightTop, rightBottomOffset - rightTop);
                }
            }
        }

        private void DrawFileViewLasso(
            CanvasDrawingSession ds, float leftTop, float leftHeight, float rightTop, float rightHeight)
        {
            var pathBuilder = new CanvasPathBuilder(ds);

            float num1 = Math.Min(4.0f, leftHeight * 0.5f);  // bezier curve control point
            float num2 = Math.Min(4.0f, rightHeight * 0.5f); // bezier curve control point
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
    }
}
