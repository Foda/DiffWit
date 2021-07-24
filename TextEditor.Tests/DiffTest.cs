using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TextEditor.Diff;
using TextEditor.Model;

namespace TextEditor.Tests
{
    [TestClass]
    public class DiffTest
    {
        [TestMethod]
        public void TestSplitDiffParsing()
        {
            string inputA =
                "This is a test file!\n" +
                "Added another line to test file\n" +
                "Adding a third line to test file";

            string inputB =
                "This is a test file!\n" +
                "Added another line to test file\n" +
                "Adding a third line to test file\n" +
                "Adding a fourth line to test file";

            List<Diff.Diff> diff = DiffFactory.GenerateDiffCache(inputA, inputB);
            SplitDiffModel splitDiff = DiffFactory.GenerateSplitDiff(diff);

            Assert.IsTrue(splitDiff.SideA.LineCount == 3);
            Assert.IsTrue(splitDiff.SideB.LineCount == 4);

            DiffTextLine addedLine = splitDiff.SideB.GetLine(3) as DiffTextLine;
            Assert.IsTrue(addedLine.BeforeLineNo == -1);
            Assert.IsTrue(addedLine.LineNo == 4);
            Assert.IsTrue(addedLine.ChangeType == DiffLineType.Insert);
        }

        [TestMethod]
        public void TestUnifiedDiffParsing_AddedRemoved()
        {
            string inputA =
                "This is a test file!\n" +
                "Added another line to test file\n" +
                "Adding a third line to test file";

            string inputB =
                "This is a test file!\n" +
                "Adding a third line to test file\n" +
                "Adding a fourth line to test file";

            List<Diff.Diff> diff = DiffFactory.GenerateDiffCache(inputA, inputB);
            DiffTextModel textModel = DiffFactory.GenerateUnifiedDiff(diff);

            Assert.IsTrue(textModel.LineCount == 5);

            DiffTextLine removedLine = textModel.GetLine(1) as DiffTextLine;
            Assert.IsTrue(removedLine.ToString() == "Added another line to test file");
            Assert.IsTrue(removedLine.BeforeLineNo == 2);
            Assert.IsTrue(removedLine.ChangeType == DiffLineType.Remove);

            DiffTextLine addedLine = textModel.GetLine(3) as DiffTextLine;
            Assert.IsTrue(addedLine.ToString() == "Adding a third line to test file");
            Assert.IsTrue(addedLine.BeforeLineNo == -1);
            Assert.IsTrue(addedLine.LineNo == 2);
            Assert.IsTrue(addedLine.ChangeType == DiffLineType.Insert);
        }
    }
}
