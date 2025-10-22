using Microsoft.EntityFrameworkCore;
using TTA_API.Data;
using TTA_API.Models;

namespace TTA_API.Services
{
    public class PlanService : IPlanService
    {
        private readonly ApplicationDbContext _context;

        public PlanService(ApplicationDbContext context)
        {
            _context = context;
        }

        // This method remains the same, but the return type is now PlanDto
        public async Task<IEnumerable<PlanDto>> GetPlansForMonthAsync(int year, int month)
        {
            var plansFromDb = await _context.Plans
                .Include(p => p.User)
                .Include(p => p.SelectedDays)
                .Where(p => p.Year == year && p.Month == month)
                .ToListAsync();

            return plansFromDb.Select(p => new PlanDto
            {
                UserId = p.UserId,
                UserName = p.User.Name,
                Month = p.Month,
                Year = p.Year,
                SelectedDays = p.SelectedDays.Select(sd => sd.Day).ToList()
            });
        }

        // This method is updated to set the flag
        public async Task SubmitPlanAsync(PlanSubmissionDto planDto)
        {
            var existingPlan = await _context.Plans
                .Include(p => p.SelectedDays)
                .FirstOrDefaultAsync(p => p.UserId == planDto.UserId && p.Year == planDto.Year && p.Month == planDto.Month);

            var allocationsExist = await _context.Allocations
                .AnyAsync(a => a.AllocationDate.Year == planDto.Year && a.AllocationDate.Month == planDto.Month + 1);

            if (existingPlan != null)
            {
                existingPlan.UpdatedTime = DateTime.UtcNow;
                _context.PlanSelectedDays.RemoveRange(existingPlan.SelectedDays);
                var newSelectedDays = planDto.SelectedDays.Select(day => new PlanSelectedDay { Day = day, PlanId = existingPlan.Id });
                _context.PlanSelectedDays.AddRange(newSelectedDays);
                if (allocationsExist)
                {
                    existingPlan.HasUpdatedSinceAllocation = true;
                }
            }
            else
            {
                var newPlan = new TTA_API.Models.Plan
                {
                    UserId = planDto.UserId,
                    Year = planDto.Year,
                    Month = planDto.Month,
                    CreatedTime = DateTime.UtcNow,
                    SelectedDays = planDto.SelectedDays.Select(day => new PlanSelectedDay { Day = day }).ToList(),
                    HasUpdatedSinceAllocation = allocationsExist // Set flag on creation
                };
                _context.Plans.Add(newPlan);
            }
            await _context.SaveChangesAsync();
        }

        // *** ADD THIS NEW METHOD ***
        public async Task<IEnumerable<string>> GetUpdatedUserNamesAsync()
        {
            var settings = await _context.SystemSettings.FindAsync(1);
            if (settings == null) return Enumerable.Empty<string>();

            var today = DateTime.UtcNow;
            int targetMonth = settings.AllocateForCurrentMonth ? today.Month - 1 : today.Month;
            if (!settings.AllocateForCurrentMonth && today.Month == 12)
            {
                targetMonth = 0;
            }
            int targetYear = today.Year;
            if (!settings.AllocateForCurrentMonth && today.Month == 12)
            {
                targetYear++;
            }

            return await _context.Plans
                .Include(p => p.User)
                .Where(p => p.Year == targetYear && p.Month == targetMonth && p.HasUpdatedSinceAllocation == true)
                .Select(p => p.User.Name)
                .Distinct()
                .ToListAsync();
        }
    }
}