using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using PsefApiOData.Models;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// Izin check scheduled job.
    /// </summary>
    public class ScheduledIzinCheck : IAsyncJob
    {
        /// <summary>
        /// Izin check scheduled job.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="service"></param>
        public ScheduledIzinCheck(PsefMySqlContext context, SmtpEmailService service)
        {
            _context = context;
            _service = service;
        }

        /// <summary>
        /// Executes the job.
        /// </summary>
        public async Task ExecuteAsync()
        {
            DateTime expiredWarningTime = DateTime.Today.AddMonths(5);
            List<Perizinan> list = await _context.Perizinan
                .Where(e =>
                    e.ExpiredAt == expiredWarningTime ||
                    e.Permohonan.StraExpiry == expiredWarningTime)
                .ToListAsync();

            foreach (Perizinan item in list)
            {
                //        send email to pemohon cc PSEF admin
                // await _service.SendEmailAsync(
                //     new MailAddress(options.Value.To, options.Value.ToDisplay ?? string.Empty),
                //     new MailAddressCollection(),
                //     "Perizinan Akan Berakhir",
                //     "Perizinan yang anda miliki akan segera berakhir. Mohon untuk mulai menyiapkan perpanjangannya.");

                //        update database ?

            }
        }

        private readonly PsefMySqlContext _context;
        private readonly SmtpEmailService _service;
    }
}
