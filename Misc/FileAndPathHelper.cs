using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace PsefApiOData.Misc
{
    internal class FileAndPathHelper
    {
        public bool ValidateFileName(IFormFile file)
        {
            //https://stackoverflow.com/questions/62771/how-do-i-check-if-a-given-string-is-a-legal-valid-file-name-under-windows#62855
            Regex invalidChars = new Regex($"[{Regex.Escape(new string(Path.GetInvalidPathChars()))}]");

            return file != null &&
                !string.IsNullOrWhiteSpace(file.FileName) &&
                !invalidChars.IsMatch(file.FileName);
        }

        public bool ValidateFileSize(IFormFile file, int maxSizeBytes)
        {
            return file.Length != 0 && file.Length <= maxSizeBytes;
        }

        public bool ValidateFileExtension(IFormFile file, string[] allowedExtensions)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Count() == 0 ||
                (!string.IsNullOrWhiteSpace(extension) &&
                allowedExtensions.Contains(extension));
        }

        public bool ValidateFileUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url) ||
                !Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                return false;
            }

            return true;
        }
    }
}