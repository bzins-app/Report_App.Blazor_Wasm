using Report_App_WASM.Shared.SerializedParameters;

namespace Report_App_WASM.Shared.Extensions
{
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

    public static class DateCalculation
    {
        public static DateTime GetCalculateDateTime(this CalulatedDateOption option)
        {
            switch (option)
            {
                case CalulatedDateOption.StartOfThisWeek:
                    return DateTime.Now.GetFisrtDayOfWeek(DayOfWeek.Monday);
                case CalulatedDateOption.EndOfThisWeek:
                    return DateTime.Now.GetLastDayOfWeek(DayOfWeek.Monday);
                case CalulatedDateOption.StartOfThisMonth:
                    return DateTime.Now.GetFisrtDayOfMonth();
                case CalulatedDateOption.EndOfThisMonth:
                    return DateTime.Now.GetLastDayOfMonth();
                case CalulatedDateOption.StartOfThisQuarter:
                    return DateTime.Now.GetFirstDayOfQuarter();
                case CalulatedDateOption.EndOfThisQuarter:
                    return DateTime.Now.GetLastDayOfQuarter();
                case CalulatedDateOption.StartOfThisYear:
                    return new DateTime(DateTime.Now.Year, 1, 1);
                case CalulatedDateOption.EndOfThisYear:
                    return new DateTime(DateTime.Now.Year, 12, 31);
                case CalulatedDateOption.Yesterday:
                    return DateTime.Now.AddDays(-1);
                case CalulatedDateOption.Tomorrow:
                    return DateTime.Now.AddDays(1);
                case CalulatedDateOption.StartOfLastWeek:
                    return DateTime.Now.GetFisrtDayOfWeek(DayOfWeek.Monday).AddDays(-7);
                case CalulatedDateOption.EndOfLastWeek:
                    return DateTime.Now.GetLastDayOfWeek(DayOfWeek.Monday).AddDays(-7);
                case CalulatedDateOption.StartOfLastMonth:
                    return DateTime.Now.AddMonths(-1).GetFisrtDayOfMonth();
                case CalulatedDateOption.EndOfLastMonth:
                    return DateTime.Now.AddMonths(-1).GetLastDayOfMonth();
                case CalulatedDateOption.LastOpeningDay:
                    return DateTime.Now.DayOfWeek == DayOfWeek.Monday
                        ? DateTime.Now.AddDays(-3)
                        : DateTime.Now.AddDays(-1);
                default:
                    return DateTime.Now;
            }

        }
    }
}
