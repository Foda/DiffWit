using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextEditor.Utils
{
    public static class FileHelper
    {
        private const int TAB_TO_SPACE_COUNT = 4;

        public static string GetFileName(string path)
        {
            // Our own implementation because Path.GetFileName can fail if path has invalid characters,
            // however since files referred to in commits may have come from another platform where
            // those characters ARE valid, we have to cope with it
            // https://jira.atlassian.com/browse/SRCTREEWIN-325

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
            // https://jira.atlassian.com/browse/SRCTREEWIN-325
            // https://jira.atlassian.com/browse/SRCTREEWIN-490
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
            // https://jira.atlassian.com/browse/SRCTREEWIN-325

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
    }
}
