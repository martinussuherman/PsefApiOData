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
    public class FileOperation
    {
        public FileOperation(IWebHostEnvironment environment)
        {
            _environment = environment;
            _validation = new FileAndPathHelper();
        }

        public async Task<IActionResult> UploadFile(
            IUrlHelper urlHelper,
            IFormFile file,
            string[] pathSegment,
            string[] permittedExtensions)
        {
            string uploaded = await CopyUploadedFile(
                file,
                pathSegment,
                permittedExtensions);

            if (string.IsNullOrEmpty(uploaded))
            {
                return new BadRequestResult();
            }

            return new CreatedResult(
                string.Empty,
                urlHelper.Content($"~/{string.Join('/', pathSegment)}/{uploaded}"));
        }

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
            string[] allowedExtensions)
        {
            if (!_validation.ValidateFileNameAndLength(file) ||
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
