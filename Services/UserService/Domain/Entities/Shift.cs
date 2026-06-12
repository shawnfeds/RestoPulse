namespace RestoPulse.UserService.Domain.Entities;

public class Shift
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public User User { get; private set; } = null!;
    public DateTime ClockInTime { get; private set; }
    public DateTime? ClockOutTime { get; private set; }
    public bool IsLate { get; private set; }
    public int OvertimeMinutes { get; private set; }
    public int RegularMinutes { get; private set; }
    public string Status { get; private set; } = "Active"; // Active, Completed
    public DateOnly Date { get; private set; }
    public int? ScheduledShiftTypeId { get; private set; }
    public ShiftType? ScheduledShiftType { get; private set; }
    public string? Notes { get; private set; }

    private Shift() { }

    public static Shift ClockIn(int userId, DateTime clockInTime, ShiftType? scheduledShift = null, string? notes = null)
    {
        bool isLate = false;
        if (scheduledShift != null)
        {
            // Compare clock in time of day (local time representation, using TimeOfDay)
            // with shift start time plus a 15-minute grace period.
            var timeOfDay = clockInTime.TimeOfDay;
            isLate = timeOfDay > scheduledShift.StartTime.Add(TimeSpan.FromMinutes(15));
        }

        return new Shift
        {
            UserId = userId,
            ClockInTime = clockInTime,
            IsLate = isLate,
            Status = "Active",
            Date = DateOnly.FromDateTime(clockInTime),
            ScheduledShiftTypeId = scheduledShift?.Id,
            Notes = notes
        };
    }

    public void ClockOut(DateTime clockOutTime, ShiftType? scheduledShift = null)
    {
        ClockOutTime = clockOutTime;
        Status = "Completed";

        var totalMinutes = (int)(clockOutTime - ClockInTime).TotalMinutes;
        if (totalMinutes < 0) totalMinutes = 0;

        int scheduledDuration = 480; // default to 8 hours (480 minutes) if no schedule
        if (scheduledShift != null)
        {
            var end = scheduledShift.EndTime;
            var start = scheduledShift.StartTime;
            if (end < start)
            {
                scheduledDuration = (int)(end.Add(TimeSpan.FromDays(1)) - start).TotalMinutes;
            }
            else
            {
                scheduledDuration = (int)(end - start).TotalMinutes;
            }
        }

        if (totalMinutes > scheduledDuration)
        {
            OvertimeMinutes = totalMinutes - scheduledDuration;
            RegularMinutes = scheduledDuration;
        }
        else
        {
            OvertimeMinutes = 0;
            RegularMinutes = totalMinutes;
        }
    }
}
