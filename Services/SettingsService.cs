using TTA_API.Data;
using TTA_API.Models;

namespace TTA_API.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;

        public SettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SystemSettings> GetSettingsAsync()
        {
            // There will only ever be one row of settings, with Id = 1
            return await _context.SystemSettings.FindAsync(1);
        }

        public async Task UpdateSettingsAsync(SystemSettingsDto settingsDto)
        {
            var settings = await _context.SystemSettings.FindAsync(1);

            if (settings != null)
            {
                settings.DepartureLabel = settingsDto.DepartureLabel;
                settings.ArrivalLabel = settingsDto.ArrivalLabel;
                settings.TripPrice = settingsDto.TripPrice;
                settings.AllocateForCurrentMonth = settingsDto.AllocateForCurrentMonth;
                settings.UserListViewEnabled = settingsDto.UserListViewEnabled;
                settings.UpdatedTime = DateTime.UtcNow; // Set update time
                settings.UpdatedBy = settingsDto.ActorUserId; // Set 

                await _context.SaveChangesAsync();
            }
        }
    }
}