using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextEditor.Utils
{
    public enum LineEndingType
    {
        Unknown,
        Win32,
        Unix
    }

    public static class FileHelper
    {
        private const int TAB_TO_SPACE_COUNT = 4;

        public static string GetFileName(string path)
        {
            // Our own implementation because Path.GetFileName can fail if path has invalid characters,
            // however since files referred to in commits may have come from another platform where
            // those characters ARE valid, we have to cope with it
            if (path == null)
                return null;

            try
            {
                return Path.GetFileName(path);
            }
            catch (Exception)
            {
                var lastSlash = path.LastIndexOfAny(new[] { '/', '\\' });
                if (lastSlash == -1)
                    return path;

                if (lastSlash + 1 >= path.Length)
                    return String.Empty;

                return path.Substring(lastSlash + 1);
            }
        }

        public static string GetDirectoryName(string path)
        {
            // Our own implementation because Path.GetDirectoryName can fail if path has invalid characters
            // or if the path of a file is too long
            // however since files referred to in commits may have come from another platform where
            // those characters ARE valid, we have to cope with it
            if (path == null)
                return null;

            try
            {
                return Path.GetDirectoryName(path);
            }
            catch (Exception)
            {
                var lastSlash = path.LastIndexOfAny(new[] { '/', '\\' });
                if (lastSlash == -1)
                    return String.Empty;

                if (lastSlash == 0)
                    return String.Empty;

                return path.Substring(0, lastSlash);
            }
        }

        public static string ConvertTabsToSpaces(string line)
        {
            return line.Replace("\t", new string(' ', TAB_TO_SPACE_COUNT));
        }

        public static string CombinePath(string path1, string path2)
        {
            // Our own implementation because GeneralHelper.CombinePath can fail if path has invalid characters,
            // however since files referred to in commits may have come from another platform where
            // those characters ARE valid, we have to cope with it

            try
            {
                // TODO shouldn't this use Path.GetFullPath(blah) to produce a simplified path 
                //as it is sometimes used during stirng comparisons frm parts of code that will only produce simplified paths.
                return Path.Combine(path1, path2);
            }
            catch (Exception)
            {
                // Concatenation isn't perfect but it's better than nothing
                if (!path1.EndsWith("\\"))
                    return path1 + "\\" + path2;

                return path1 + path2;
            }
        }

        public static string GetAbsoluteFilename(string path, string repoRelativeFilename)
        {
            // Make sure we have standardised the path separators
            var stdpath = repoRelativeFilename.Replace('/', '\\');

            return CombinePath(path, stdpath);
        }

        /// <summary>
        /// Gets the next line from a string, starting at a given position
        /// The returned string doesn't include the line ending, but we return
        /// what kind it was. The next point to read from is passed back in nextLineStart
        /// </summary>
        /// <param name="diff"></param>
        /// <param name="start"></param>
        /// <param name="le"></param>
        /// <param name="nextLineStart"></param>
        /// <returns></returns>
        public static string GetNextLineFromString(string diff, int start, out LineEndingType le, out int nextLineStart)
        {
            int pos = diff.Length;

            le = LineEndingType.Unknown;
            nextLineStart = diff.Length;

            int windowsLineEndingPos = diff.IndexOf("\r\n", start);
            int unixLineEnding = diff.IndexOf('\n', start);

            // make sure we get the closest line ending value
            if (windowsLineEndingPos != -1 &&
                windowsLineEndingPos < unixLineEnding &&
                windowsLineEndingPos + 1 < diff.Length)
            {
                le = LineEndingType.Win32;
                nextLineStart = windowsLineEndingPos + 2;
                pos = windowsLineEndingPos;
            }
            else if (unixLineEnding != -1)
            {
                le = LineEndingType.Unix;
                nextLineStart = unixLineEnding + 1;
                pos = unixLineEnding;
            }

            int len = pos - start;
            return len > 0 ? diff.Substring(start, len) : string.Empty;
        }
    }
}
