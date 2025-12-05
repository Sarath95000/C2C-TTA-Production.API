using TTA_API.Models;

namespace TTA_API.Services
{
    public interface IAllocationService
    {
        // Change the return types here to use the new AllocationDto
        Task<IEnumerable<AllocationDto>> GetAllocationsAsync();
        Task<IEnumerable<AllocationDto>> GenerateAllocationsAsync(int year, int month, int actorUserId);
    }
}