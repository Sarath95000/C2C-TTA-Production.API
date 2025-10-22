using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTA_API.Models
{
    public class SystemSettings
    {
        [Key]
        public int SettingsId { get; set; } = 1;

        [Required]
        public string DepartureLabel { get; set; }

        [Required]
        public string ArrivalLabel { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TripPrice { get; set; }

        public bool AllocateForCurrentMonth { get; set; }

        public bool UserListViewEnabled { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? UpdatedBy { get; set; }
    }
}