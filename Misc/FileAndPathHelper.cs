using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace PsefApiOData.Misc
{
    internal class FileAndPathHelper
    {
        public bool ValidateFileNameAndLength(IFormFile file)
        {
            return file != null &&
                !string.IsNullOrWhiteSpace(file.FileName) &&
                file.Length != 0;
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