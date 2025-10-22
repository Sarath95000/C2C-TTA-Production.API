using TTA_API.Models;

namespace TTA_API.Services
{
    public interface IPlanService
    {
        // Change the return type here to use the new DTO
        Task<IEnumerable<PlanDto>> GetPlansForMonthAsync(int year, int month);
        Task SubmitPlanAsync(PlanSubmissionDto planDto);
        Task<IEnumerable<string>> GetUpdatedUserNamesAsync();
    }
}