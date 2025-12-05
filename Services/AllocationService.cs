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


        // Your updated service method
        public async Task<IEnumerable<AllocationDto>> GenerateAllocationsAsync(int year, int month, int actorUserId)
        {
            // The old logic for calculating targetMonth and targetYear from the current date is removed.
            // We now use the parameters directly.
            int targetYear = year;
            // The month from the request is 1-based (Jan=1). The database stores it as 0-based (Jan=0).
            // We adjust it here for all queries against the Plan entity.
            int targetMonth = month;

            var settings = await _context.SystemSettings.FindAsync(1);

            var plansForMonth = await _context.Plans
                .Include(p => p.User)
                .Include(p => p.SelectedDays)
                // Use the parameters in the query
                .Where(p => p.Year == targetYear && p.Month == targetMonth)
                .ToListAsync();

            var allUniqueDays = plansForMonth
                .SelectMany(p => p.SelectedDays.Select(d => d.Day))
                .Distinct()
                // The DateTime constructor correctly uses a 1-based month, so we use the original 'month' parameter.
                .Select(day => new DateTime(targetYear, month, day, 0, 0, 0, DateTimeKind.Utc))
                .ToList();

            var allDaysToAllocate = allUniqueDays.OrderBy(d => d.Day).ToList();

            var existingAllocationsCount = await _context.Allocations
                .GroupBy(a => a.BookerUserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var newAllocations = new List<TTA_API.Models.Allocation>();

            var eligibleBookers = plansForMonth
                .Select(p => p.User)
                .DistinctBy(u => u.Id)
                .ToList();

            var random = new Random();
            eligibleBookers = eligibleBookers.OrderBy(u => random.Next()).ToList();

            int lastBookerIndex = -1;

            foreach (var date in allDaysToAllocate)
            {
                var availableUsers = plansForMonth
                    .Where(p => p.SelectedDays.Any(d => d.Day == date.Day))
                    .Select(p => p.User)
                    .ToList();

                if (!availableUsers.Any()) continue;

                User booker;

                // --- NEW: Prioritize single traveler as their own booker ---
                if (availableUsers.Count == 1)
                {
                    // If there's only one person traveling, they are automatically the booker.
                    booker = availableUsers.First();
                }
                else
                {
                    // --- EXISTING: Use Round-Robin for multiple travelers ---
                    User roundRobinBooker = null;
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
                                roundRobinBooker = potentialBooker;
                                lastBookerIndex = potentialBookerIndex; // Remember this person's index for the next day's allocation.
                                break; // We've found our booker, so we can exit the loop.
                            }
                        }
                    }

                    // Fallback: This should rarely be needed, but if the round-robin somehow fails,
                    // we'll revert to picking the available user with the fewest existing bookings.
                    booker = roundRobinBooker ?? availableUsers
                        .OrderBy(u => existingAllocationsCount.GetValueOrDefault(u.Id, 0))
                        .First();
                }
                // --- END OF LOGIC CHANGE ---

                // --- REVISED: Trip Type Logic ---
                // The trip type is now determined by the day of the week using our new helper method.
                var tripType = GetTripTypeByDayOfWeek(date, settings.DepartureLabel, settings.ArrivalLabel);

                var allocation = new TTA_API.Models.Allocation
                {
                    CreatedBy = actorUserId,
                    CreatedTime = DateTime.Now,
                    AllocationDate = date,
                    BookerUserId = booker.Id,
                    TripType = tripType,
                    Travelers = availableUsers.Select(u => new AllocationTraveler { UserId = u.Id }).ToList(),
                };

                newAllocations.Add(allocation);
                // We still increment the count. This is useful for the fallback logic and for general stats.
                existingAllocationsCount[booker.Id] = existingAllocationsCount.GetValueOrDefault(booker.Id, 0) + 1;
            }

            // Clear previous allocations for the selected month and year before adding new ones.
            // Note: AllocationDate.Month is 1-based, so we use the original 'month' parameter.
            await _context.Allocations
                .Where(a => a.AllocationDate.Year == targetYear && a.AllocationDate.Month == month)
                .ExecuteDeleteAsync();

            _context.Allocations.AddRange(newAllocations);
            await _context.SaveChangesAsync();

            // Reset the "has updated" flag for the correct plans.
            var plansToReset = await _context.Plans
                .Where(p => p.Year == targetYear && p.Month == targetMonth)
                .ToListAsync();

            foreach (var plan in plansToReset)
            {
                plan.HasUpdatedSinceAllocation = false;
            }
            await _context.SaveChangesAsync();

            await new EmailService(_context).SendEmailAsync(false,null,month,year);
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