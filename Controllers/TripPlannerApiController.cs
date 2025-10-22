using Microsoft.AspNetCore.Mvc;
using TTA_API.Services;
// Add your project's using statements for services and models here
// using TripPlanner.Api.Services;
// using TripPlanner.Api.Models;

namespace TTA_API.Controllers
{
    [ApiController]
    [Route("api")]
    public class TripPlannerApiController : ControllerBase
    {
        // These would be your services, injected via dependency injection.
        // They contain the business logic to interact with the database.
        private readonly IUserService _userService;
        private readonly IPlanService _planService;
        private readonly IAllocationService _allocationService;
        private readonly ISettingsService _settingsService;
        private readonly IHolidayService _holidayService;

        private readonly ILogger<TripPlannerApiController> _logger;


        // --- THIS IS THE CORRECTED CODE ---
        public TripPlannerApiController(
            ILogger<TripPlannerApiController> logger,
            IUserService userService,
            IPlanService planService,
            IAllocationService allocationService,
            ISettingsService settingsService,
            IHolidayService holidayService
            )
        {
            _logger = logger;
            _userService = userService;
            _planService = planService;
            _allocationService = allocationService;
            _settingsService = settingsService;
            _holidayService = holidayService;
        }

        // --- User Endpoints ---

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] Models.UserUpdateRequestDto userDto)
        {
            var result = await _userService.UpdateUserAsync(id, userDto);
            if (!result) return NotFound();
            return Ok(new { message = "User updated successfully!" });
        }

        [HttpPost("users")]
        public async Task<IActionResult> AddUser([FromBody] Models.UserCreateRequestDto userDto)
        {
            var newUser = await _userService.AddUserAsync(userDto);
            return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, newUser);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "User deleted successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            var user = await _userService.AuthenticateAsync(loginDto.Identifier, loginDto.Pin);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }
            return Ok(user);
        }

        // --- Plan Endpoints ---

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans([FromQuery] int year, [FromQuery] int month)
        {
            var plans = await _planService.GetPlansForMonthAsync(year, month);
            return Ok(plans);
        }

        [HttpPost("plans")]
        public async Task<IActionResult> SubmitPlan([FromBody] Models.PlanSubmissionDto planDto)
        {
            await _planService.SubmitPlanAsync(planDto);
            return Ok(new { message = "Plan submitted successfully!" });
        }

        // --- Allocation Endpoints ---

        [HttpGet("allocations")]
        public async Task<IActionResult> GetAllocations()
        {
            var allocations = await _allocationService.GetAllocationsAsync();
            return Ok(allocations);
        }

        [HttpPost("allocations/generate")]
        public async Task<IActionResult> GenerateAllocations()
        {
            var newAllocations = await _allocationService.GenerateAllocationsAsync();
            return Ok(newAllocations);
        }

        // --- System Settings Endpoints ---

        [HttpGet("settings")]
        public async Task<IActionResult> GetSystemSettings()
        {
            var settings = await _settingsService.GetSettingsAsync();
            return Ok(settings);
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSystemSettings([FromBody] Models.SystemSettingsDto settingsDto)
        {
            await _settingsService.UpdateSettingsAsync(settingsDto);
            return Ok(new { message = "Settings updated successfully!" });
        }

        // --- Holiday Endpoints ---

        [HttpGet("holidays")]
        public async Task<IActionResult> GetHolidays()
        {
            var holidays = await _holidayService.GetHolidaysAsync();
            return Ok(holidays);
        }

        [HttpPut("holidays")]
        public async Task<IActionResult> UpdateHolidays([FromBody] UpdateHolidaysRequestDto request)
        {
            await _holidayService.UpdateHolidaysAsync(request.HolidayDates, request.ActorUserId);
            return Ok(new { message = "Holidays updated successfully!" });
        }

        [HttpGet("plan-updates")]
        public async Task<IActionResult> GetPlanUpdates()
        {
            var userNames = await _planService.GetUpdatedUserNamesAsync();
            return Ok(userNames);
        }
    }

    // --- Data Transfer Objects (DTOs) ---
    // These are classes used to shape the data sent to and from the API.

    public class LoginRequestDto
    {
        public string Identifier { get; set; } // Can be name or email
        public string Pin { get; set; }
    }

    public class UserUpdateRequestDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool SendEmail { get; set; }
        public string? NewPin { get; set; } // Optional: only if changing
        public string? CurrentPin { get; set; } // Required if changing PIN
    }

    public class UserCreateRequestDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Role { get; set; } // Maps to your Role Enum
        public bool SendEmail { get; set; }
    }

    public class PlanSubmissionDto
    {
        public int UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public List<int> SelectedDays { get; set; }
    }

    public class SystemSettingsDto
    {
        public string DepartureLabel { get; set; }
        public string ArrivalLabel { get; set; }
        public decimal TripPrice { get; set; }
        public bool AllocateForCurrentMonth { get; set; }
        public bool UserListViewEnabled { get; set; }
    }
    public class UpdateHolidaysRequestDto
    {
        public List<string> HolidayDates { get; set; }
        public int ActorUserId { get; set; }
    }
}