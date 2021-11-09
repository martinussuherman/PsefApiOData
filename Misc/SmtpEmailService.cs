using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// SMTP email service.
    /// </summary>
    public class SmtpEmailService
    {
        /// <summary>
        /// SMTP email service.
        /// </summary>
        /// <param name="logger">Logger service.</param>
        /// <param name="options">SMTP options.</param>
        public SmtpEmailService(ILogger<SmtpEmailService> logger, IOptions<SmtpOptions> options)
        {
            _logger = logger;
            _options = options;

            _client = new SmtpClient
            {
                Host = _options.Value.Host,
                Port = _options.Value.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = _options.Value.UseSSL
            };

            if (!string.IsNullOrEmpty(_options.Value.Password))
            {
                _client.Credentials = new System.Net.NetworkCredential(
                    _options.Value.Login,
                    _options.Value.Password);
            }
            else
            {
                _client.UseDefaultCredentials = true;
            }
        }

        /// <summary>
        /// Send email via STMP.
        /// </summary>
        /// <param name="to">Address to send email to.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="htmlMessage">Email html message.</param>
        /// <param name="cc">Email cc addresses. Multiple email addresses must be separated with a comma character (",")</param>
        /// <returns>Task.</returns>
        public Task SendEmailAsync(string to, string subject, string htmlMessage, string cc = null)
        {
            _logger.LogInformation($"Sending email: {to}, subject: {subject}, message: {htmlMessage}");

            try
            {
                string from = string.IsNullOrEmpty(_options.Value.From) ?
                    _options.Value.Login :
                    _options.Value.From;

                MailMessage mail = new MailMessage(from, to)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = htmlMessage
                };

                if (!string.IsNullOrEmpty(cc))
                {
                    mail.CC.Add(cc);
                    cc = string.Empty;
                }

                _client.Send(mail);
                _logger.LogInformation(
                    $"Email: {to}, cc: {cc}, subject: {subject}, message: {htmlMessage} successfully sent");

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception {ex} during sending email: {to}, cc: {cc}, subject: {subject}");
                throw;
            }
        }

        private readonly ILogger<SmtpEmailService> _logger;
        private readonly IOptions<SmtpOptions> _options;
        private readonly SmtpClient _client;
    }
}