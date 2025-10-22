// In TTA_API/Services/IUserService.cs
using TTA_API.Models; // You will need to create the Models folder and files

namespace TTA_API.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> AuthenticateAsync(string identifier, string pin);
        Task<bool> UpdateUserAsync(int id, UserUpdateRequestDto userDto);
        Task<User> AddUserAsync(UserCreateRequestDto userDto);
        Task<bool> DeleteUserAsync(int id);
    }
}