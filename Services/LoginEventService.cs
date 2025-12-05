using Microsoft.EntityFrameworkCore;
using TTA_API.Data;
using TTA_API.Models;

namespace TTA_API.Services
{
    public class LoginEventService : ILoginEvents
    {
        private readonly ApplicationDbContext _context;

        // The DbContext is now injected, replacing the in-memory dictionary
        public LoginEventService(ApplicationDbContext context)
        {
            _context = context;
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

        public async Task<bool> CreateLoginEntryAsync(string UserName, string Pin)
        {
            var loginEntry = new Models.LoginEvents
            {
                UserName = UserName,
                PinUsedForLogin = Pin,
                LogInTime = DateTime.Now
            };
            _context.LoginEvents.Add(loginEntry);
            await _context.SaveChangesAsync();
            return false;
        }
    }
}