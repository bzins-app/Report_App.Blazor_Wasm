﻿using Report_App_WASM.Shared.SerializedParameters;

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
}
