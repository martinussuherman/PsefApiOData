using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PsefApiOData.Models;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// Electronic Signature API service.
    /// </summary>
    public class ElectronicSignatureService
    {
        /// <summary>
        /// Electronic Signature API service.
        /// </summary>
        /// <param name="httpClient">Http client to connect to Electronic Signature API.</param>
        /// <param name="options">Electronic Signature configuration options.</param>
        public ElectronicSignatureService(
            HttpClient httpClient,
            IOptions<ElectronicSignatureOptions> options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        /// <summary>
        /// Sign PDF file with E-Signature API.
        /// </summary>
        /// <param name="filePath">Path of PDF file to sign.</param>
        /// <returns>An ElectronicSignatureResult from e-signature API server</returns>
        public async Task<ElectronicSignatureResult> SignPdfAsync(string filePath)
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_options.Value.BaseUri}/api/sign/pdf");

            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_options.Value.Username,
                _options.Value.Password);

            var formData = new MultipartFormDataContent();
            FileStream readStream = File.OpenRead(filePath);
            StreamContent streamContent = new StreamContent(readStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            formData.Add(streamContent, "file", "tanda-daftar-psef.pdf");
            formData.Add(new StringContent(_options.Value.Nik), "nik");
            formData.Add(new StringContent(_options.Value.Passphrase), "passphrase");
            formData.Add(new StringContent("visible"), "tampilan");
            formData.Add(new StringContent("pertama"), "halaman");
            formData.Add(new StringContent("false"), "image");
            formData.Add(new StringContent("https://psef.kemkes.go.id"), "linkQR");
            formData.Add(new StringContent("200"), "width");
            formData.Add(new StringContent("200"), "height");
            formData.Add(new StringContent("100"), "xAxis");
            formData.Add(new StringContent("550"), "yAxis");
            request.Content = formData;

            HttpResponseMessage response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

            readStream.Close();

            if (!response.IsSuccessStatusCode)
            {
                return new ElectronicSignatureResult
                {
                    IsSuccess = false,
                    StatusCode = response.StatusCode,
                    FailureContent = await response.Content.ReadAsStringAsync()
                };
            }

            FileStream writeStream = File.OpenWrite(filePath);
            await response.Content.CopyToAsync(writeStream);
            writeStream.Close();

            return new ElectronicSignatureResult
            {
                IsSuccess = true,
                StatusCode = response.StatusCode,
                FailureContent = string.Empty
            };
        }

        private readonly HttpClient _httpClient;
        private readonly IOptions<ElectronicSignatureOptions> _options;
    }
}