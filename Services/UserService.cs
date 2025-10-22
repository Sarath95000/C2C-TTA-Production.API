using Microsoft.EntityFrameworkCore;
using TTA_API.Data;
using TTA_API.Models;

namespace TTA_API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        // The DbContext is now injected, replacing the in-memory dictionary
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> AuthenticateAsync(string identifier, string pin)
        {
            // Convert the identifier to lower case once to be efficient
            var lowerIdentifier = identifier.ToLower();

            // Queries the real database for a matching user using a translatable case-insensitive search
            return await _context.Users.FirstOrDefaultAsync(u =>
                (u.Name.ToLower() == lowerIdentifier ||
                 u.Email.ToLower() == lowerIdentifier) &&
                u.Pin == pin);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            // Gets all users from the database
            return await _context.Users.OrderBy(u => u.Id).ToListAsync();
        }

        public async Task<User> AddUserAsync(UserCreateRequestDto userDto)
        {

            Random random = new Random();
            int userpin = random.Next(1000, 10000);

            var newUser = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Role = (Role)userDto.Role,
                SendEmail = userDto.SendEmail,
                Pin = ((Role)userDto.Role == Role.SystemAdmin || (Role)userDto.Role == Role.AllocationAdmin) ? "5361" : userpin.ToString(),
                CreatedTime = DateTime.Now,
                CreatedBy = userDto.ActorUserId,
                // Audit fields can be added here if needed
            };

            // Adds the new user to the context and saves it to the database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            if (newUser.SendEmail) 
            {
                await new EmailService(_context).SendEmailAsync(true, newUser); 
            }
            return newUser;
        }

        public async Task<bool> UpdateUserAsync(int id, UserUpdateRequestDto userDto)
        {
            // Finds the user in the database by their ID
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Updates the properties
            user.Name = userDto.Name;
            user.Email = userDto.Email;
            user.SendEmail = userDto.SendEmail;
            user.UpdatedBy = userDto.ActorUserId;
            user.UpdatedTime = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(userDto.NewPin) && !string.IsNullOrEmpty(userDto.CurrentPin) && user.Pin == userDto.CurrentPin)
            {
                user.Pin = userDto.NewPin;
            }

            // Saves the changes to the database
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Removes the user and saves the change to the database
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}