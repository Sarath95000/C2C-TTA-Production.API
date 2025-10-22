// In TTA_API/Services/ISettingsService.cs
using TTA_API.Models;

namespace TTA_API.Services
{
    public interface ISettingsService
    {
        Task<SystemSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(SystemSettingsDto settingsDto);
    }
}