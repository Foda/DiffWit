using Microsoft.Toolkit.Uwp.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Diff;
using TextEditor.Utils;
using Windows.Storage;

namespace DiffWit.ViewModel
{
    public abstract class BaseDiffViewModel : ReactiveObject, IDiffViewModel
    {
        internal List<Diff> _diffCache = new List<Diff>();

        private string _fileA = "";
        public string FileA
        {
            get { return _fileA; }
            private set { this.RaiseAndSetIfChanged(ref _fileA, value); }
        }

        private string _fileB = "";
        public string FileB
        {
            get { return _fileB; }
            private set { this.RaiseAndSetIfChanged(ref _fileB, value); }
        }

        public int ChangeCount { get { return _diffCache.Count; } }

        public async Task GenerateDiffAsync(string fileA, string fileB)
        {
            fileA = fileA.Replace("/", "\\");
            fileB = fileB.Replace("/", "\\");

            var fileAFolder = await StorageFolder.GetFolderFromPathAsync(
                fileA.Substring(0, fileA.LastIndexOf('\\')));

            var fileBFolder = await StorageFolder.GetFolderFromPathAsync(
                fileB.Substring(0, fileB.LastIndexOf('\\')));

            var fileAFile = await fileAFolder.GetFileAsync(FileHelper.GetFileName(fileA));
            var fileBFile = await fileBFolder.GetFileAsync(FileHelper.GetFileName(fileB));

            await GenerateDiffFromFilesAsync(fileAFile, fileBFile);
        }

        private async Task GenerateDiffFromFilesAsync(StorageFile fileA, StorageFile fileB)
        {
            var fileAText = await Windows.Storage.FileIO.ReadTextAsync(fileA);
            var fileBText = await Windows.Storage.FileIO.ReadTextAsync(fileB);

            fileAText = fileAText.Replace("\r\n", "\n");
            fileBText = fileBText.Replace("\r\n", "\n");

            _diffCache = DiffFactory.GenerateDiffCache(fileAText, fileBText);

            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                FileA = fileA.Path;
                FileB = fileB.Path;

                ProcessDiff();
            });
        }

        internal abstract void ProcessDiff();
    }
}
