using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextEditor.Diff;
using TextEditor.Utils;
using Windows.Storage;

namespace DiffWit.Utils
{
    public class DiffCacheUtil
    {
        public static async Task<List<Diff>> GenerateDiffCache(string fileA, string fileB)
        {
            if (string.IsNullOrEmpty(fileA) || string.IsNullOrEmpty(fileB))
            {
                return new List<Diff>();
            }

            fileA = fileA.Replace("/", "\\");
            fileB = fileB.Replace("/", "\\");

            var fileAFolder = await StorageFolder.GetFolderFromPathAsync(
                fileA.Substring(0, fileA.LastIndexOf('\\')));

            var fileBFolder = await StorageFolder.GetFolderFromPathAsync(
                fileB.Substring(0, fileB.LastIndexOf('\\')));

            var fileAFile = await fileAFolder.GetFileAsync(FileHelper.GetFileName(fileA));
            var fileBFile = await fileBFolder.GetFileAsync(FileHelper.GetFileName(fileB));

            var fileAText = await Windows.Storage.FileIO.ReadTextAsync(fileAFile);
            var fileBText = await Windows.Storage.FileIO.ReadTextAsync(fileBFile);

            fileAText = fileAText.Replace("\r\n", "\n");
            fileBText = fileBText.Replace("\r\n", "\n");

            return DiffFactory.GenerateDiffCache(fileAText, fileBText);
        }
    }
}
