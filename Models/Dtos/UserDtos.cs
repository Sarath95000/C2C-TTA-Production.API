namespace TTA_API.Models
{
    public class UserUpdateRequestDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool SendEmail { get; set; }
        public string? NewPin { get; set; }
        public string? CurrentPin { get; set; }
        public int ActorUserId { get; set; } // Who is making the change
    }

    public class UserCreateRequestDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public bool SendEmail { get; set; }
        public int ActorUserId { get; set; } // Who is creating the user
    }
}