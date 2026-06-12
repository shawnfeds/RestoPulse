namespace RestoPulse.UserService.Domain.Entities;

public class ShiftType
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty; // Morning, Evening, Night
    public TimeSpan StartTime { get; private set; } // e.g. 09:00:00
    public TimeSpan EndTime { get; private set; }   // e.g. 17:00:00

    private ShiftType() { }

    public static ShiftType Create(string name, TimeSpan startTime, TimeSpan endTime)
    {
        return new ShiftType
        {
            Name = name.Trim(),
            StartTime = startTime,
            EndTime = endTime
        };
    }
}
