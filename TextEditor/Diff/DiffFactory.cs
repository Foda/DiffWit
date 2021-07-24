using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            diff_match_patch dmp = new diff_match_patch();
            (string chars1, string chars2, List<string> lines) a = dmp.diff_linesToChars(textA, textB);

            List<Diff> diffs = dmp.diff_main(a.chars1, a.chars2, true);

            // Convert the diff back to original text.
            dmp.diff_charsToLines(diffs, a.lines);

            // Eliminate freak matches (e.g. blank lines)
            // dmp.diff_cleanupSemantic(diffs);

            return diffs;
        }

        public static DiffTextModel GenerateUnifiedDiff(List<Diff> diffs)
        {
            // Generate the model
            DiffTextModel textModel = new();

            int beforeLineNo = 1;
            int afterLineNo = 1;

            foreach (Diff d in diffs)
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

                string[] splitLines = d.text.TrimEnd('\n').Split('\n');
                if (splitLines.Length > 0)
                {
                    for (int i = 0; i < splitLines.Length; i++)
                    {
                        bool useBeforeLineNo = true;
                        bool useAfterLineNo = true;

                        switch (d.operation)
                        {
                            case Operation.DELETE:
                                {
                                    useAfterLineNo = false;
                                }
                                break;
                            case Operation.INSERT:
                                {
                                    useBeforeLineNo = false;
                                }
                                break;
                            case Operation.EQUAL:
                            default:
                                break;
                        }

                        DiffTextLine newLine = new DiffTextLine(splitLines[i], diffType)
                        {
                            BeforeLineNo = useBeforeLineNo ? beforeLineNo++ : -1,
                            LineNo = useAfterLineNo ? afterLineNo++ : -1
                        };
                        textModel.InsertLine(newLine);
                    }
                }
            }

            return textModel;
        }
        
        public static SplitDiffModel GenerateSplitDiff(List<Diff> diffs)
        {
            // Generate the model
            List<DiffTextLine> sideABuffer = new List<DiffTextLine>();
            List<DiffTextLine> sideBBuffer = new List<DiffTextLine>();

            DiffTextModel sideAModel = new DiffTextModel();
            DiffTextModel sideBModel = new DiffTextModel();

            List<IAnchorPos> diffAnchors = new List<IAnchorPos>();

            foreach (Diff d in diffs)
            {
                bool hasAddedDiffAnchor = false;
                foreach (string line in d.text.SplitToLines())
                {
                    switch (d.operation)
                    {
                        case Operation.DELETE:
                            {
                                // removed on left
                                DiffTextLine newLine = new DiffTextLine(line, DiffLineType.Remove);
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
                                DiffTextLine newLine = new DiffTextLine(line, DiffLineType.Insert);
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

            int leftLineNo = 1;
            int rightLineNo = 1;

            int lineNoCount = Math.Max(sideABuffer.Count, sideBBuffer.Count);

            DiffTextLine leftLine = null;
            DiffTextLine rightLine = null;
            DiffTextLine emptyTextLine = new DiffTextLine("", DiffLineType.Empty);

            // Make sure we visit every line 
            for (int i = 0; i < lineNoCount; i++)
            {
                leftLine = leftLineNo <= sideABuffer.Count ? sideABuffer[leftLineNo - 1] : null;
                rightLine = rightLineNo <= sideBBuffer.Count ? sideBBuffer[rightLineNo - 1] : null;
                
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

                    leftLineNo++; // only advance the left line counter because empty lines don't count
                }
                else if (leftLine.ChangeType == DiffLineType.Unchanged &&
                         rightLine.ChangeType == DiffLineType.Insert)
                {
                    rightLine.LineNo = rightLineNo;

                    // added on right, but didn't have existing change on left
                    sideAModel.InsertLine(emptyTextLine);
                    sideBModel.InsertLine(rightLine);

                    rightLineNo++; // only advance the right line counter because empty lines don't count
                }
            }

            return new SplitDiffModel(sideAModel, sideBModel, diffAnchors);
        }
    }
}
