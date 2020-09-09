using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PsefApiOData.Models;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// Counter helpers.
    /// </summary>
    public class CounterHelper
    {
        /// <summary>
        /// Counter helpers.
        /// </summary>
        /// <param name="context">Database context.</param>
        public CounterHelper(PsefMySqlContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets next form number
        /// </summary>
        /// <param name="type">Type of form number to get.</param>
        /// <returns>Next form number.</returns>
        public async Task<string> GetFormNumber(CounterType type)
        {
            bool done = false;
            Counter counter = null;
            int currentNumber = 0;
            string currentCounterDate;

            while (!done)
            {
                counter = await _context.Counter.FindAsync((ushort)type);
                currentCounterDate = string.IsNullOrEmpty(counter.DateFormat) ?
                    counter.LastValueDate :
                    string.Format(counter.DateFormat, DateTime.Now);
                currentNumber = currentCounterDate == counter.LastValueDate ?
                    counter.LastValueNumber + 1 :
                    1;

                counter.LastValueDate = currentCounterDate;
                counter.LastValueNumber = currentNumber;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    ex.Entries.Single().Reload();
                    continue;
                }

                done = true;
            }

            return string.Format(counter.DisplayFormat, DateTime.Now, currentNumber);
        }

        private readonly PsefMySqlContext _context;
    }
}