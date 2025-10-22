namespace TTA_API.Models
{
    public class AllocationTraveler
    {
        public int AllocationId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Allocation Allocation { get; set; }
        public User User { get; set; }
    }
}