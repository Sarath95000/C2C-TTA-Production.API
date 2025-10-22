namespace TTA_API.Models
{
    // This class represents the join table for the many-to-many relationship
    public class PlanSelectedDay
    {
        public int PlanId { get; set; }
        public int Day { get; set; }

        // Navigation property
        public Plan Plan { get; set; }
    }
}