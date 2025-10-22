namespace TTA_API.Models
{
    // This DTO defines the shape of a Plan as the frontend expects it.
    public class PlanDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<int> SelectedDays { get; set; }
    }
}