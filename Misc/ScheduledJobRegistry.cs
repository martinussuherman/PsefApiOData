using System;
using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PsefApiOData.Models;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// Registry of scheduled job.
    /// </summary>
    public class ScheduledJobRegistry : Registry
    {
        /// <summary>
        /// Registry of scheduled job.
        /// </summary>
        /// <param name="provider">Service provider.</param>
        public ScheduledJobRegistry(IServiceProvider provider)
        {
            NonReentrantAsDefault();
            using IServiceScope scope = provider.CreateScope();
            PsefMySqlContext context = new PsefMySqlContext(
                scope.ServiceProvider.GetRequiredService<DbContextOptions<PsefMySqlContext>>());
            SmtpEmailService emailService = scope.ServiceProvider.GetRequiredService<SmtpEmailService>();
            Schedule(new ScheduledIzinCheck(context, emailService)).ToRunEvery(1).Days().At(3, 12);
        }
    }
}
