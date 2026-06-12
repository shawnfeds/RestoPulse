namespace RestoPulse.UserService.Domain.Entities;

public class UserSchedule
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public User User { get; private set; } = null!;
    public DateOnly Date { get; private set; }
    public int ShiftTypeId { get; private set; }
    public ShiftType ShiftType { get; private set; } = null!;

    private UserSchedule() { }

    public static UserSchedule Create(int userId, DateOnly date, int shiftTypeId)
    {
        return new UserSchedule
        {
            UserId = userId,
            Date = date,
            ShiftTypeId = shiftTypeId
        };
    }
}
