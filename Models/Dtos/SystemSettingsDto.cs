namespace TTA_API.Models
{
    public class SystemSettingsDto
    {
        public string DepartureLabel { get; set; }
        public string ArrivalLabel { get; set; }
        public decimal TripPrice { get; set; }
        public bool AllocateForCurrentMonth { get; set; }
        public bool UserListViewEnabled { get; set; }
        public int ActorUserId { get; set; } // Who is saving the settings
    }
}