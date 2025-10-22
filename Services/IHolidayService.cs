namespace TTA_API.Services
{
    public interface IHolidayService
    {
        Task<IEnumerable<string>> GetHolidaysAsync();
        Task UpdateHolidaysAsync(List<string> holidayDates, int actorUserId);

    }
}