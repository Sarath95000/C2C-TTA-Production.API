namespace TTA_API.Models
{
    // A simple DTO for a traveler's details
    public class TravelerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // This DTO perfectly matches the 'Allocation' type in the frontend's types.ts
    public class AllocationDto
    {
        public string Date { get; set; }
        public int BookerId { get; set; }
        public string BookerName { get; set; }
        public List<TravelerDto> Travelers { get; set; }
        public string? TripType { get; set; }
    }
}