using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Model;
using TextEditor.Utils;

namespace TextEditor.Diff
{
    public struct SplitDiffModel
    {
        public DiffTextModel SideA { get; }
        public DiffTextModel SideB { get; }
        public List<IAnchorPos> DiffAnchors { get; }

        public SplitDiffModel(DiffTextModel sideA, DiffTextModel sideB,
            List<IAnchorPos> diffAnchors)
        {
            SideA = sideA;
            SideB = sideB;
            DiffAnchors = diffAnchors;
        }
    }

    public class DiffFactory
    {
        public static List<Diff> GenerateDiffCache(string textA, string textB)
        {
            var dmp = new diff_match_patch();
            Object[] a = dmp.diff_linesToChars(textA, textB);

            List<Diff> diffs = dmp.diff_main((string)a[0], (string)a[1], true);

            // Convert the diff back to original text.
            dmp.diff_charsToLines(diffs, (List<string>)a[2]);

            // Eliminate freak matches (e.g. blank lines)
            // dmp.diff_cleanupSemantic(diffs);

            return diffs;
        }

        public static DiffTextModel GenerateUnifiedDiff(List<Diff> diffs)
        {
            // Generate the model
            var textModel = new DiffTextModel();

            foreach (var d in diffs)
            {
                DiffLineType diffType = DiffLineType.Empty;
                switch (d.operation)
                {
                    case Operation.DELETE:
                        {
                            diffType = DiffLineType.Remove;
                        }
                        break;
                    case Operation.EQUAL:
                        {
                            diffType = DiffLineType.Unchanged;
                        }
                        break;
                    case Operation.INSERT:
                        {
                            diffType = DiffLineType.Insert;
                        }
                        break;
                }

                var splitLines = d.text.TrimEnd('\n').Split('\n');
                if (splitLines.Length > 0)
                {
                    for (int i = 0; i < splitLines.Length; i++)
                    {
                        var newLine = new DiffTextLine(splitLines[i], diffType);
                        newLine.LineNo = i + 1;
                        textModel.InsertLine(newLine);
                    }
                }
            }

            return textModel;
        }
        
        public static SplitDiffModel GenerateSplitDiff(List<Diff> diffs)
        {
            // Generate the model
            var sideABuffer = new List<DiffTextLine>();
            var sideBBuffer = new List<DiffTextLine>();
           
            var sideAModel = new DiffTextModel();
            var sideBModel = new DiffTextModel();

            var diffAnchors = new List<IAnchorPos>();

            foreach (var d in diffs)
            {
                bool hasAddedDiffAnchor = false;
                foreach (var line in d.text.SplitToLines())
                {
                    switch (d.operation)
                    {
                        case Operation.DELETE:
                            {
                                // removed on left
                                var newLine = new DiffTextLine(line, DiffLineType.Remove);
                                sideABuffer.Add(newLine);

                                if (!hasAddedDiffAnchor)
                                {
                                    diffAnchors.Add(sideAModel.CreateAnchor(newLine));
                                    hasAddedDiffAnchor = true;
                                }
                            }
                            break;
                        case Operation.EQUAL:
                            {
                                sideABuffer.Add(new DiffTextLine(line, DiffLineType.Unchanged));
                                sideBBuffer.Add(new DiffTextLine(line, DiffLineType.Unchanged));
                            }
                            break;
                        case Operation.INSERT:
                            {
                                // added on the right
                                var newLine = new DiffTextLine(line, DiffLineType.Insert);
                                sideBBuffer.Add(newLine);

                                if (!hasAddedDiffAnchor)
                                {
                                    diffAnchors.Add(sideBModel.CreateAnchor(newLine));
                                    hasAddedDiffAnchor = true;
                                }
                            }
                            break;
                    }
                }
            }

            // Add empty space for lines on the left were added but we don't have a
            // removal on our side
            var emptyTextLine = new DiffTextLine("", DiffLineType.Empty);

            int leftLineNo = 1;
            int rightLineNo = 1;

            DiffTextLine leftLine = null;
            DiffTextLine rightLine = null;

            // Make sure we visit every line 
            while (leftLineNo < sideABuffer.Count || rightLineNo < sideBBuffer.Count)
            {
                leftLine = leftLineNo < sideABuffer.Count ? sideABuffer[leftLineNo - 1] : null;
                rightLine = rightLineNo < sideBBuffer.Count ? sideBBuffer[rightLineNo - 1] : null;
                
                if (leftLine == null && rightLine != null)
                {
                    rightLine.LineNo = rightLineNo;
                    sideBModel.InsertLine(rightLine);
                    rightLineNo++;
                    continue;
                }

                if (leftLine != null && rightLine == null)
                {
                    leftLine.LineNo = leftLineNo;
                    sideAModel.InsertLine(leftLine);
                    leftLineNo++;
                    continue;
                }

                if (leftLine.ChangeType == DiffLineType.Unchanged &&
                    rightLine.ChangeType == DiffLineType.Unchanged)
                {
                    leftLine.LineNo = leftLineNo;
                    rightLine.LineNo = rightLineNo;

                    // easymode: unchanged lines
                    sideAModel.InsertLine(leftLine);
                    sideBModel.InsertLine(rightLine);

                    leftLineNo++;
                    rightLineNo++;
                }
                else if (leftLine.ChangeType == DiffLineType.Remove &&
                         rightLine.ChangeType == DiffLineType.Insert)
                {
                    leftLine.LineNo = leftLineNo;
                    rightLine.LineNo = rightLineNo;

                    // removed left, added right, so it's 1-to-1
                    sideAModel.InsertLine(leftLine);
                    sideBModel.InsertLine(rightLine);

                    leftLineNo++;
                    rightLineNo++;
                }
                else if (leftLine.ChangeType == DiffLineType.Remove &&
                         rightLine.ChangeType == DiffLineType.Unchanged)
                {
                    leftLine.LineNo = leftLineNo;

                    // removed on left, but wasn't replaced with anything on right
                    sideAModel.InsertLine(leftLine);
                    sideBModel.InsertLine(emptyTextLine);

                    leftLineNo++; // only advance the left line counter
                }
                else if (leftLine.ChangeType == DiffLineType.Unchanged &&
                         rightLine.ChangeType == DiffLineType.Insert)
                {
                    rightLine.LineNo = rightLineNo;

                    // added on right, but didn't have existing change on left
                    sideAModel.InsertLine(emptyTextLine);
                    sideBModel.InsertLine(rightLine);

                    rightLineNo++; // only advance the right line counter
                }
            }

            return new SplitDiffModel(sideAModel, sideBModel, diffAnchors);
        }
    }
}
