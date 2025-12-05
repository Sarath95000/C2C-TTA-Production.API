using TTA_API.Models;

namespace TTA_API.Services
{
    public interface IEmailService
    {
        public  Task<IEnumerable<TravelSchedules>> SendEmailAsync(bool isForNewUserCreation = false,User user = null, int month = default, int year = default);

    }
}