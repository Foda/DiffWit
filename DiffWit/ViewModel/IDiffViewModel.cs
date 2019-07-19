using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Diff;

namespace DiffWit.ViewModel
{
    public interface IDiffViewModel
    {
        int ChangeCount { get; }

        string FileA { get; }
        string FileB { get; }

        Task GenerateDiffAsync(string fileA, string fileB);
    }
}
