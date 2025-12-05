using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTA_API.Models
{

    public class LoginEvents
    {
        [Key] // Marks Id as the primary key
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }
        
        [Required]
        public string PinUsedForLogin { get; set; }

        [Required]
        public DateTime LogInTime { get; set; }
    }
}