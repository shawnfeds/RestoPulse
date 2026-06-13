using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Domain.Entities;

namespace RestoPulse.UserService.Infrastructure.Persistence;

public static class UserDbSeeder
{
    public static async Task SeedAsync(UserDbContext db)
    {
        if (await db.Shifts.AnyAsync()) return;

        // ShiftTypes are seeded via EF migrations (HasData), so query them directly
        var morning = await db.ShiftTypes.FindAsync(1); // 09:00 - 17:00
        var evening = await db.ShiftTypes.FindAsync(2); // 17:00 - 01:00
        var night   = await db.ShiftTypes.FindAsync(3); // 21:00 - 05:00

        var shifts = new List<Shift>();
        var today  = DateTime.UtcNow.Date;
        var start  = today.AddDays(-34); // ~5 weeks of history

        // User IDs: 1=Owner, 2=Manager, 3=Chef, 4=Server
        // Schedules: Owner rarely works weekdays, Manager morning, Chef evening, Server splits
        for (int dayOffset = 0; dayOffset <= 34; dayOffset++)
        {
            var date = start.AddDays(dayOffset);
            var dow  = date.DayOfWeek;

            // Skip Sundays for Owner (he's the owner, takes weekends off)
            // Manager works Mon-Sat morning shift
            // Chef works Mon-Sat evening shift
            // Server works Mon-Fri + alternating Saturdays

            // ── Manager (userId=2): Morning shift Mon-Sat
            if (dow != DayOfWeek.Sunday)
            {
                var isLateDay   = dayOffset % 7 == 3; // every ~week, manager is late
                var hasOvertime = dayOffset % 5 == 0; // every 5 days, some overtime

                var clockIn  = date.AddHours(9).AddMinutes(isLateDay ? 22 : Random(2, 8));
                var clockOut = hasOvertime
                    ? date.AddHours(18).AddMinutes(Random(15, 45))
                    : date.AddHours(17).AddMinutes(Random(-10, 10));

                var shift = Shift.ClockIn(2, clockIn, morning, isLateDay ? "Traffic delay" : null);
                shift.ClockOut(clockOut, morning);
                shifts.Add(shift);
            }

            // ── Chef (userId=3): Evening shift Mon-Sat
            if (dow != DayOfWeek.Sunday)
            {
                var isLateDay   = dayOffset % 9 == 5;
                var hasOvertime = dayOffset % 4 == 0;

                var clockIn  = date.AddHours(17).AddMinutes(isLateDay ? 20 : Random(0, 10));
                var clockOut = hasOvertime
                    ? date.AddDays(1).AddHours(2).AddMinutes(Random(0, 30))  // next day, well past 01:00
                    : date.AddDays(1).AddHours(1).AddMinutes(Random(-15, 15));

                var shift = Shift.ClockIn(3, clockIn, evening, null);
                shift.ClockOut(clockOut, evening);
                shifts.Add(shift);
            }

            // ── Server (userId=4): Mon-Fri + every other Saturday (alternates between morning & evening)
            bool serverWorksToday = dow is not DayOfWeek.Sunday &&
                                    !(dow == DayOfWeek.Saturday && dayOffset % 14 >= 7);
            if (serverWorksToday)
            {
                bool useEvening  = dayOffset % 3 == 0;
                var  shiftType   = useEvening ? evening : morning;
                var  isLateDay   = dayOffset % 11 == 6;
                var  hasOvertime = dayOffset % 6 == 0;

                DateTime clockIn, clockOut;
                if (useEvening)
                {
                    clockIn  = date.AddHours(17).AddMinutes(isLateDay ? 18 : Random(0, 10));
                    clockOut = hasOvertime
                        ? date.AddDays(1).AddHours(2).AddMinutes(Random(0, 20))
                        : date.AddDays(1).AddHours(1).AddMinutes(Random(-20, 20));
                }
                else
                {
                    clockIn  = date.AddHours(9).AddMinutes(isLateDay ? 25 : Random(0, 10));
                    clockOut = hasOvertime
                        ? date.AddHours(18).AddMinutes(Random(30, 90))
                        : date.AddHours(17).AddMinutes(Random(-5, 15));
                }

                var shift = Shift.ClockIn(4, clockIn, shiftType, null);
                shift.ClockOut(clockOut, shiftType);
                shifts.Add(shift);
            }

            // ── Owner (userId=1): Spot checks — Mon/Wed/Fri morning only
            if (dow is DayOfWeek.Monday or DayOfWeek.Wednesday or DayOfWeek.Friday)
            {
                var clockIn  = date.AddHours(10).AddMinutes(Random(0, 30));
                var clockOut = date.AddHours(14).AddMinutes(Random(0, 60)); // shorter days
                // Owner has no fixed shift type, just unscheduled
                var shift = Shift.ClockIn(1, clockIn, null, "Routine check-in");
                shift.ClockOut(clockOut, null);
                shifts.Add(shift);
            }
        }

        db.Shifts.AddRange(shifts);
        await db.SaveChangesAsync();
    }

    private static int Random(int min, int max)
    {
        // Deterministic pseudo-random to avoid seeding issues across runs
        return min + Math.Abs(System.Environment.TickCount % (max - min + 1));
    }
}
