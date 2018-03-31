using System;
using System.IO;

namespace DllRefChanger.Utils
{
    public static class PathHelper
    {
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public static string GetFileNameWithoutExtention(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetRelativePath(string basePath, string absolutePath)
        {          
            if (!File.Exists(absolutePath))
            {
                throw new ArgumentException("文件不存在呀!");
            }
            if (!File.Exists(basePath))
            {
                if (!Directory.Exists(basePath))
                {
                    throw new ArgumentException($"{basePath} 路径不存在。");
                }
            }

            if (Path.GetPathRoot(absolutePath) != Path.GetPathRoot(basePath))
            {
                return absolutePath;
            }

            return MakeRelativePath(basePath, absolutePath);

        }



        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
                throw new ArgumentNullException(nameof(fromPath));
            if (string.IsNullOrEmpty(toPath))
                throw new ArgumentNullException(nameof(toPath));

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
            { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
