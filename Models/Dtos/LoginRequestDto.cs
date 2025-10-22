namespace TTA_API.Models
{
    public class LoginRequestDto
    {
        public string Identifier { get; set; } // Can be name or email
        public string Pin { get; set; }
    }
}