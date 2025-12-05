// In TTA_API/Services/IUserService.cs
using TTA_API.Models; // You will need to create the Models folder and files

namespace TTA_API.Services
{
    public interface ILoginEvents
    {
        Task<bool> CreateLoginEntryAsync(string UserName, string Pin);
    }
}