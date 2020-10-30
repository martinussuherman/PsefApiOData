using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// File operation helpers.
    /// </summary>
    public class FileOperation
    {
        /// <summary>
        /// File operation helpers.
        /// </summary>
        /// <param name="environment">Webhost environment.</param>
        public FileOperation(IWebHostEnvironment environment)
        {
            _environment = environment;
            _validation = new FileAndPathHelper();
        }

        /// <summary>
        /// Upload file.
        /// </summary>
        /// <param name="urlHelper">Url helper.</param>
        /// <param name="file">Uploaded file.</param>
        /// <param name="pathSegment">File path segment relative to wwwroot.</param>
        /// <param name="permittedExtensions">Permitted file extensions.</param>
        /// <param name="maxSizeBytes">Max file size in bytes.</param>
        /// <returns>File url on success, or bad request on fail.</returns>
        public async Task<IActionResult> UploadFile(
            IUrlHelper urlHelper,
            IFormFile file,
            string[] pathSegment,
            string[] permittedExtensions,
            int maxSizeBytes)
        {
            string uploaded = await CopyUploadedFile(
                file,
                pathSegment,
                permittedExtensions,
                maxSizeBytes);

            if (string.IsNullOrEmpty(uploaded))
            {
                return new BadRequestResult();
            }

            return new CreatedResult(
                string.Empty,
                urlHelper.Content($"~/{string.Join('/', pathSegment)}/{uploaded}"));
        }

        /// <summary>
        /// Delete file.
        /// </summary>
        /// <param name="validateUrl">Url validation function.</param>
        /// <param name="request">Http request.</param>
        /// <param name="relativeUrl">Relative url of file to delete.</param>
        /// <returns></returns>
        public IActionResult DeleteFile(
            Func<string, bool> validateUrl,
            HttpRequest request,
            string relativeUrl)
        {
            if (!validateUrl(relativeUrl))
            {
                return new BadRequestResult();
            }

            StringBuilder builder = new StringBuilder(relativeUrl);

            builder
                .Replace(request.PathBase.Value, string.Empty);

            string cleanedUrl = builder.ToString();
            string filePath = Path.Combine(
                _environment.WebRootPath,
                Path.Combine(cleanedUrl.Split('/')));

            if (!File.Exists(filePath))
            {
                return new BadRequestResult();
            }

            File.Delete(filePath);
            return new OkObjectResult(cleanedUrl);
        }

        private async Task<string> CopyUploadedFile(
            IFormFile file,
            string[] pathSegment,
            string[] allowedExtensions,
            int maxSizeBytes)
        {
            if (!_validation.ValidateFileName(file) ||
                !_validation.ValidateFileSize(file, maxSizeBytes) ||
                !_validation.ValidateFileExtension(file, allowedExtensions))
            {
                return string.Empty;
            }

            string folderPath = Path.Combine(pathSegment.Prepend(_environment.WebRootPath).ToArray());

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, file.FileName);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return WebUtility.HtmlEncode(file.FileName);
        }

        private readonly IWebHostEnvironment _environment;
        private readonly FileAndPathHelper _validation;
    }
}
