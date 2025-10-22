using System.ComponentModel.DataAnnotations;

namespace TTA_API.Models
{
    public class Holiday
    {
        [Key]
        public DateTime HolidayDate { get; set; }
        public DateTime CreatedTime { get; set; }
        public int CreatedBy { get; set; }
    }
}