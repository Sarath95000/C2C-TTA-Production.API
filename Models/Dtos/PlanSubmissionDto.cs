namespace TTA_API.Models
{
    public class PlanSubmissionDto
    {
        public int UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public List<int> SelectedDays { get; set; }
    }
}