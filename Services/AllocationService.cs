using Microsoft.EntityFrameworkCore;
using TTA_API.Data;
using TTA_API.Models;

namespace TTA_API.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly ApplicationDbContext _context;

        public AllocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // FIX: This method now returns a list of DTOs instead of the raw database models
        public async Task<IEnumerable<AllocationDto>> GetAllocationsAsync()
        {
            var allocationsFromDb = await _context.Allocations
                .Include(a => a.Booker)
                .Include(a => a.Travelers)
                    .ThenInclude(at => at.User)
                .OrderBy(a => a.AllocationDate)
                .ToListAsync();

            // Map the database models to the clean DTO to break the circular reference
            return allocationsFromDb.Select(a => new AllocationDto
            {
                Date = a.AllocationDate.ToString("yyyy-MM-dd"),
                BookerId = a.BookerUserId,
                BookerName = a.Booker.Name,
                Travelers = a.Travelers.Select(t => new TravelerDto { Id = t.UserId, Name = t.User.Name }).ToList(),
                TripType = a.TripType
            });
        }

        // In your backend C# service file (e.g., AllocationService.cs or TripPlannerService.cs)

        // In your backend C# service file (e.g., AllocationService.cs or a similar name)

        // Add this helper method to your class, for example, as a private method.
        // It cleanly separates the logic for determining the trip type.
        private string GetTripTypeByDayOfWeek(DateTime date, string departureLabel, string arrivalLabel)
        {
            // DayOfWeek is an enum where Sunday is 0, Monday is 1, etc.
            DayOfWeek day = date.DayOfWeek;

            // Sunday through Wednesday are designated as Departures.
            if (day >= DayOfWeek.Sunday && day <= DayOfWeek.Wednesday)
            {
                return departureLabel;
            }
            else // Thursday, Friday, and Saturday are designated as Arrivals.
            {
                return arrivalLabel;
            }
        }


        public async Task<IEnumerable<AllocationDto>> GenerateAllocationsAsync()
        {
            var settings = await _context.SystemSettings.FindAsync(1);
            var today = DateTime.UtcNow;

            // This part remains unchanged: determines the target month and year for allocation.
            int targetMonth = settings.AllocateForCurrentMonth ? today.Month - 1 : today.Month;
            int targetYear = today.Year;
            if (!settings.AllocateForCurrentMonth && today.Month == 12)
            {
                targetMonth = 0; // The plan's month is 0-indexed, so 0 is January
                targetYear++;
            }

            var plansForMonth = await _context.Plans
                .Include(p => p.User)
                .Include(p => p.SelectedDays)
                .Where(p => p.Year == targetYear && p.Month == targetMonth)
                .ToListAsync();

            var allUniqueDays = plansForMonth
                .SelectMany(p => p.SelectedDays.Select(d => d.Day))
                .Distinct()
                .Select(day => new DateTime(targetYear, targetMonth + 1, day, 0, 0, 0, DateTimeKind.Utc))
                .ToList();

            // MODIFICATION: Sort days chronologically. This provides a more natural and predictable
            // allocation order compared to sorting by the number of travelers.
            var allDaysToAllocate = allUniqueDays.OrderBy(d => d.Day).ToList();

            var existingAllocationsCount = await _context.Allocations
                .GroupBy(a => a.BookerUserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var newAllocations = new List<TTA_API.Models.Allocation>();

            // --- NEW LOGIC: Setup for Fair Round-Robin Allocation ---
            // 1. Get a distinct list of all users who have submitted a plan for the month.
            //    These are the only users eligible to be a booker.
            var eligibleBookers = plansForMonth
                .Select(p => p.User)
                .DistinctBy(u => u.Id) // Using DistinctBy to ensure each user appears only once.
                .ToList();

            // 2. Shuffle this master list of bookers randomly. This is crucial for fairness,
            //    as it ensures the starting point of the cycle is not based on user ID or name.
            var random = new Random();
            eligibleBookers = eligibleBookers.OrderBy(u => random.Next()).ToList();

            // 3. This index will track the last person in our shuffled list who was assigned a booking.
            int lastBookerIndex = -1;
            // --- END OF NEW SETUP ---

            foreach (var date in allDaysToAllocate)
            {
                var availableUsers = plansForMonth
                    .Where(p => p.SelectedDays.Any(d => d.Day == date.Day))
                    .Select(p => p.User)
                    .ToList();

                if (!availableUsers.Any()) continue;

                // --- REVISED: Round-Robin Booker Selection Logic ---
                // This block replaces the previous LINQ query that used alphabetical sorting.
                User booker = null;
                if (eligibleBookers.Any())
                {
                    // We'll loop through our entire master list of bookers to find one who is available today.
                    for (int i = 0; i < eligibleBookers.Count; i++)
                    {
                        // Start searching from the person *after* the last one who booked.
                        // The modulo operator (%) ensures the search wraps around to the beginning of the list.
                        int potentialBookerIndex = (lastBookerIndex + 1 + i) % eligibleBookers.Count;
                        var potentialBooker = eligibleBookers[potentialBookerIndex];

                        // Check if this person from our master list is actually available on the current date.
                        if (availableUsers.Any(u => u.Id == potentialBooker.Id))
                        {
                            booker = potentialBooker;
                            lastBookerIndex = potentialBookerIndex; // Remember this person's index for the next day's allocation.
                            break; // We've found our booker, so we can exit the loop.
                        }
                    }
                }

                // Fallback: This should rarely be needed, but if the round-robin somehow fails,
                // we'll revert to picking the available user with the fewest existing bookings.
                if (booker == null)
                {
                    booker = availableUsers
                       .OrderBy(u => existingAllocationsCount.GetValueOrDefault(u.Id, 0))
                       .First();
                }
                // --- END OF REVISED LOGIC ---

                // --- REVISED: Trip Type Logic ---
                // The trip type is now determined by the day of the week using our new helper method.
                var tripType = GetTripTypeByDayOfWeek(date, settings.DepartureLabel, settings.ArrivalLabel);

                var allocation = new TTA_API.Models.Allocation
                {
                    AllocationDate = date,
                    BookerUserId = booker.Id,
                    TripType = tripType,
                    Travelers = availableUsers.Select(u => new AllocationTraveler { UserId = u.Id }).ToList()
                };

                newAllocations.Add(allocation);
                // We still increment the count. This is useful for the fallback logic and for general stats.
                existingAllocationsCount[booker.Id] = existingAllocationsCount.GetValueOrDefault(booker.Id, 0) + 1;
            }

            // This part remains unchanged: clears old allocations and saves the new ones.
            await _context.Allocations
                .Where(a => a.AllocationDate.Year == targetYear && a.AllocationDate.Month == targetMonth + 1)
                .ExecuteDeleteAsync();

            _context.Allocations.AddRange(newAllocations);
            await _context.SaveChangesAsync();

            var plansToReset = await _context.Plans
                .Where(p => p.Year == targetYear && p.Month == targetMonth)
                .ToListAsync();

            foreach (var plan in plansToReset)
            {
                plan.HasUpdatedSinceAllocation = false;
            }
            await _context.SaveChangesAsync();

            await new EmailService(_context).SendEmailAsync();
            return await GetAllocationsAsync();
        }

        private string GetTripType(DateTime date, List<TTA_API.Models.Plan> plans, string departureLabel, string arrivalLabel)
        {
            var weekNumber = System.Globalization.ISOWeek.GetWeekOfYear(date);
            var yearOfWeek = System.Globalization.ISOWeek.GetYear(date);

            var daysInSameWeek = plans
                .SelectMany(p => p.SelectedDays.Select(d => new DateTime(p.Year, p.Month + 1, d.Day)))
                .Where(d => System.Globalization.ISOWeek.GetWeekOfYear(d) == weekNumber && System.Globalization.ISOWeek.GetYear(d) == yearOfWeek)
                .OrderBy(d => d)
                .ToList();

            var isFirstTripOfWeek = daysInSameWeek.FirstOrDefault() == date;

            if (date.DayOfWeek >= DayOfWeek.Wednesday && date.DayOfWeek <= DayOfWeek.Saturday)
            {
                return arrivalLabel;
            }

            if (!isFirstTripOfWeek)
            {
                return arrivalLabel;
            }

            return departureLabel;
        }
    }
}