using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Add this using statement

namespace TTA_API.Models
{
    public class Allocation
    {
        [Key]
        public int Id { get; set; }
        public DateTime AllocationDate { get; set; }

        // This is the foreign key property
        public int BookerUserId { get; set; }
        public string? TripType { get; set; }

        // This is the "navigation property" linked to the foreign key above
        [ForeignKey("BookerUserId")] // This attribute explicitly links the two
        public User Booker { get; set; }

        public ICollection<AllocationTraveler> Travelers { get; set; }
        public DateTime CreatedTime { get; set; }
        public int CreatedBy { get; set; } // The Admin who ran the allocation
        public DateTime? UpdatedTime { get; set; }
        public int? UpdatedBy { get; set; }

    }
}