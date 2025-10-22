//using Microsoft.EntityFrameworkCore;
//using TTA_API.Data;
//using TTA_API.Models;

//namespace TTA_API.Services
//{
//    public class TravelScheduleService
//    {
//        private readonly ApplicationDbContext _context;

//        public TravelScheduleService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // A DTO (Data Transfer Object) to hold the final, formatted data for the frontend.
//        public class TravelScheduleViewModel
//        {
//            public string TravelDate { get; set; }
//            public string TripType { get; set; }
//            public string Booker { get; set; }
//            public string TravelingWith { get; set; }
//        }

//        public async Task<List<TravelScheduleViewModel>> GetUserTravelScheduleAsync(int currentUserId)
//        {
//            // The LINQ query to fetch and project the data.
//            var schedule = await _context.Allocations
//                // Eagerly load related data (Booker and Travelers) to prevent multiple database trips.
//                .Include(a => a.Booker)
//                //.Include(a => a.AllocationTravelers)
//                    //.ThenInclude(at => at.Traveler)

//                // This is the main filter: find all trips where the user is one of the travelers.
//                //.Where(a => a.AllocationTravelers.Any(at => at.TravelerId == currentUserId))

//                // Order the results chronologically, just like in the UI.
//                //.OrderBy(a => a.Date)

//                // Project the database results into our view model.
//                .Select(a => new TravelScheduleViewModel
//                {
//                    // Format the date into a user-friendly string (e.g., "Monday, October 6").
//                    TravelDate = a.Date.ToString("dddd, MMMM d"),
//                    TripType = a.TripType,

//                    Booker = (a.BookerId == currentUserId) ? "YOU" : a.Booker.Name,

//                    TravelingWith = string.Join(", ", a.AllocationTravelers
//                                                        .Where(at => at.TravelerId != currentUserId)
//                                                        .Select(at => at.Traveler.Name))
//                })
//                .ToListAsync();

//            foreach (var trip in schedule)
//            {
//                if (string.IsNullOrEmpty(trip.TravelingWith))
//                {
//                    trip.TravelingWith = "Just me!";
//                }
//            }

//            return schedule;
//        }
//    }

//    public class User
//    {
//        public int Id { get; set; }
//        public string Name { get; set; }
//    }

//    public class Allocation
//    {
//        public int Id { get; set; }
//        public DateTime Date { get; set; }
//        public string TripType { get; set; }
//        public int BookerId { get; set; }
//        public User Booker { get; set; }
//        public ICollection<AllocationTraveler> AllocationTravelers { get; set; }
//    }

//    public class AllocationTraveler
//    {
//        public int AllocationId { get; set; }
//        public Allocation Allocation { get; set; }
//        public int TravelerId { get; set; }
//        public User Traveler { get; set; }
//    }
//}