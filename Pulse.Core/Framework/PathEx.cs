using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Core
{
    public static class PathEx
    {
        public static string ChangeName(string filePath, string newName)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
                return newName;

            return Path.Combine(directory, newName);
        }

        public static string GetMultiDotComparableExtension(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            int index = fileName.LastIndexOf('.');
            if (index < 0)
                return string.Empty;

            if (index > 0)
            {
                int secondIndex = fileName.LastIndexOf('.', index - 1);
                if (secondIndex > 0)
                    return fileName.Substring(secondIndex).ToLower();
            }

            return fileName.Substring(index).ToLower();
        }

        public static string ChangeMultiDotExtension(string filePath, string targetExtension)
        {
            string extension = GetMultiDotComparableExtension(filePath);
            if (extension != string.Empty)
                filePath = filePath.Substring(0, filePath.Length - extension.Length);

            if (string.IsNullOrEmpty(targetExtension))
                return filePath;

            if (targetExtension[0] != '.')
                targetExtension = '.' + targetExtension;

            return filePath + targetExtension;
        }
    }
}