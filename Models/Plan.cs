using System.ComponentModel.DataAnnotations;

namespace TTA_API.Models
{
    public class Plan
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        // *** ADD THIS NEW PROPERTY ***
        public bool HasUpdatedSinceAllocation { get; set; } = false;

        public User User { get; set; }
        public ICollection<PlanSelectedDay> SelectedDays { get; set; }
        public DateTime CreatedTime { get; set; }
        // UserId already serves as the creator/owner, so CreatedBy is redundant.
        public DateTime? UpdatedTime { get; set; }
    }
}