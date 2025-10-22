using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTA_API.Models
{
    public enum Role
    {
        AllocationAdmin,
        User,
        SystemAdmin
    }

    public class User
    {
        [Key] // Marks Id as the primary key
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        public Role Role { get; set; }

        public bool SendEmail { get; set; }

        [Required]
        [StringLength(4)]
        public string Pin { get; set; }

        public DateTime CreatedTime { get; set; }
        public int? CreatedBy { get; set; } 
        public DateTime? UpdatedTime { get; set; }
        public int? UpdatedBy { get; set; }
    }
}