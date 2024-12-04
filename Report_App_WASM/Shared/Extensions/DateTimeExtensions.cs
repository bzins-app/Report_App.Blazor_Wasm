namespace Report_App_WASM.Shared.Extensions;

public static class DateTimeExtensions
{
    public static DateTime GetFisrtDayOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        var diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }

    public static DateTime GetLastDayOfWeek(this DateTime dt, DayOfWeek firstDay)
    {
        var firstDayInWeek = GetFisrtDayOfWeek(dt, firstDay);
        return firstDayInWeek.AddDays(7);
    }

    public static DateTime GetLastDayOfMonth(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
    }

    public static DateTime GetFisrtDayOfMonth(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, 1);
    }

    public static DateTime GetFirstDayOfQuarter(this DateTime dt)
    {
        var currentQuarter = (dt.Month - 1) / 3 + 1;
        return new DateTime(dt.Year, (currentQuarter - 1) * 3 + 1, 1);
    }

    public static DateTime GetLastDayOfQuarter(this DateTime dt)
    {
        var currentQuarter = (dt.Month - 1) / 3 + 1;
        DateTime firstDayOfQuarter = new(dt.Year, (currentQuarter - 1) * 3 + 1, 1);
        return firstDayOfQuarter.AddMonths(3).AddDays(-1);
    }
}