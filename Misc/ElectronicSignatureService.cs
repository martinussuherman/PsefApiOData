using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

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
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task SignPdfAsync(string filePath)
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
            formData.Add(new StringContent("invisible"), "tampilan");
            formData.Add(new StringContent("1"), "page");

            request.Content = formData;

            HttpResponseMessage response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            readStream.Close();

            FileStream writeStream = File.OpenWrite(filePath);
            await response.Content.CopyToAsync(writeStream);
            writeStream.Close();
        }

        private readonly HttpClient _httpClient;
        private readonly IOptions<ElectronicSignatureOptions> _options;
    }
}