namespace RestoPulse.UserService.Contracts;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, UserResponse User);
public record UserResponse(int Id, string Username, string FullName, string Role, bool IsActive, DateTime CreatedAt);
public record CreateUserRequest(string Username, string Password, string FullName, string Role);
public record UpdateUserRequest(string FullName, string Role);
public record ChangePasswordRequest(string? CurrentPassword, string NewPassword);
public record ClockStatusResponse(bool IsClockedIn, ShiftResponse? ActiveShift);
public record ShiftResponse(
    int Id,
    int UserId,
    string FullName,
    DateTime ClockInTime,
    DateTime? ClockOutTime,
    bool IsLate,
    int OvertimeMinutes,
    int RegularMinutes,
    string Status,
    DateOnly Date,
    string? ShiftName,
    string? Notes);

public record ShiftTypeResponse(int Id, string Name, string StartTime, string EndTime);
public record UserScheduleResponse(int Id, int UserId, string FullName, DateOnly Date, int ShiftTypeId, string ShiftName);
public record SetScheduleRequest(int UserId, DateOnly Date, int ShiftTypeId);

public record MonthlyHoursReport(
    int UserId,
    string FullName,
    int TotalMinutesWorked,
    int TotalOvertimeMinutes,
    int TotalRegularMinutes,
    int LateInCount,
    List<ShiftResponse> Shifts);
