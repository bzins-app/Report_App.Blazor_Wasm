namespace Report_App_WASM.Shared.Extensions;

public static class DateCalculation
{
    public static DateTime GetCalculateDateTime(this CalulatedDateOption option)
    {
        return option switch
        {
            CalulatedDateOption.StartOfThisWeek => DateTime.Now.GetFisrtDayOfWeek(DayOfWeek.Monday),
            CalulatedDateOption.EndOfThisWeek => DateTime.Now.GetLastDayOfWeek(DayOfWeek.Monday),
            CalulatedDateOption.StartOfThisMonth => DateTime.Now.GetFisrtDayOfMonth(),
            CalulatedDateOption.EndOfThisMonth => DateTime.Now.GetLastDayOfMonth(),
            CalulatedDateOption.StartOfThisQuarter => DateTime.Now.GetFirstDayOfQuarter(),
            CalulatedDateOption.EndOfThisQuarter => DateTime.Now.GetLastDayOfQuarter(),
            CalulatedDateOption.StartOfThisYear => new DateTime(DateTime.Now.Year, 1, 1),
            CalulatedDateOption.EndOfThisYear => new DateTime(DateTime.Now.Year, 12, 31),
            CalulatedDateOption.Yesterday => DateTime.Now.AddDays(-1),
            CalulatedDateOption.Tomorrow => DateTime.Now.AddDays(1),
            CalulatedDateOption.StartOfLastWeek => DateTime.Now.GetFisrtDayOfWeek(DayOfWeek.Monday).AddDays(-7),
            CalulatedDateOption.EndOfLastWeek => DateTime.Now.GetLastDayOfWeek(DayOfWeek.Monday).AddDays(-7),
            CalulatedDateOption.StartOfLastMonth => DateTime.Now.AddMonths(-1).GetFisrtDayOfMonth(),
            CalulatedDateOption.EndOfLastMonth => DateTime.Now.AddMonths(-1).GetLastDayOfMonth(),
            CalulatedDateOption.LastOpeningDay => DateTime.Now.DayOfWeek == DayOfWeek.Monday
                ? DateTime.Now.AddDays(-3)
                : DateTime.Now.AddDays(-1),
            _ => DateTime.Now
        };
    }
}