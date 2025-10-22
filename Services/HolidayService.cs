using Microsoft.EntityFrameworkCore;
using TTA_API.Data;
using TTA_API.Models;

namespace TTA_API.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly ApplicationDbContext _context;

        public HolidayService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<string>> GetHolidaysAsync()
        {
            return await _context.Holidays
                .Select(h => h.HolidayDate.ToString("yyyy-MM-dd")).ToListAsync();
        }

        public async Task UpdateHolidaysAsync(List<string> holidayDates, int actorUserId)
        {
            // This is a "replace all" operation. Start a transaction for safety.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Remove all existing holidays
                await _context.Holidays.ExecuteDeleteAsync();

                // Add the new holidays
                var newHolidays = holidayDates.Select(dateStr => new Holiday { HolidayDate = DateTime.Parse(dateStr).ToUniversalTime(),
                    CreatedTime = DateTime.UtcNow, // Set creation time
                    CreatedBy = actorUserId
                });
                _context.Holidays.AddRange(newHolidays);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Re-throw the exception after rolling back
            }
        }
    }
}