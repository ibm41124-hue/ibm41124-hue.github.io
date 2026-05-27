using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace EthiopianCalendarPortal.Pages
{
    public partial class EthiopianCalendar : ComponentBase
    {
        protected int currentMonthIndex = 0;
        protected int inputYear = 2018;
        protected int displayedYear;
        protected List<MonthModel>? monthsData;

        protected readonly string[] weekdayHeaders = { "ሰኞ", "ማክሰ", "ረቡዕ", "ሐሙስ", "አርብ", "ቅዳሜ", "እሁድ" };

        private readonly (string Amharic, string English)[] monthNames = {
            ("መስከረም", "Meskerem"), ("ጥቅምት", "Tikimt"), ("ኅዳር", "Hidar"),
            ("ታኅሣሥ", "Tahsas"), ("ጥር", "Ter"), ("የካቲት", "Yekatit"),
            ("መጋቢት", "Megabit"), ("ሚያዝያ", "Miyazya"), ("ግንቦት", "Ginbot"),
            ("ሰኔ", "Sene"), ("ሐምሌ", "Hamle"), ("ነሐሴ", "Nehase"),
            ("ጳጉሜ", "Pagume")
        };

        private MonthModel? currentMonth => (monthsData != null && currentMonthIndex >= 0 && currentMonthIndex < monthsData.Count) ? monthsData[currentMonthIndex] : null;

        protected override void OnInitialized()
        {
            DateTime today = DateTime.UtcNow;
            inputYear = ConvertGregorianToEthiopianYear(today);
            GenerateCalendar();
            currentMonthIndex = ConvertGregorianToEthiopianMonth(today);
        }

        private int ConvertGregorianToEthiopianYear(DateTime gregorianDate)
        {
            int gcYear = gregorianDate.Year;
            int ecYear = gcYear - 7;
            
            if (gregorianDate.Month < 9 || (gregorianDate.Month == 9 && gregorianDate.Day < 11))
            {
                ecYear--;
            }
            
            return ecYear;
        }

        private int ConvertGregorianToEthiopianMonth(DateTime gregorianDate)
        {
            // Ethiopian month start dates in Gregorian calendar (non-leap year)
            // Month 0: Meskerem = Sept 11
            // Month 1: Tikimt = Oct 11
            // Month 2: Hidar = Nov 11
            // Month 3: Tahsas = Dec 11
            // Month 4: Ter = Jan 11
            // Month 5: Yekatit = Feb 11
            // Month 6: Megabit = Mar 11
            // Month 7: Miyazya = Apr 11
            // Month 8: Ginbot = May 11
            // Month 9: Sene = Jun 11
            // Month 10: Hamle = Jul 11
            // Month 11: Nehase = Aug 11
            // Month 12: Pagume = Sept 10 (5-6 days)
            
            int gcMonth = gregorianDate.Month;
            int gcDay = gregorianDate.Day;
            int ecYear = ConvertGregorianToEthiopianYear(gregorianDate);
            bool isLeapYear = ecYear % 4 == 0;
            
            // Simplified mapping based on Gregorian month
            int[] monthStartDays = { 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 10 };
            int[] mappedMonths = { 9, 10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            
            // Find which Ethiopian month this Gregorian date falls into
            for (int ecMonth = 0; ecMonth < 13; ecMonth++)
            {
                int ecMonthGcMonth = mappedMonths[ecMonth];
                int ecMonthGcDay = monthStartDays[ecMonth];
                
                // Adjust for leap year (Pagume shifts by 1 day in leap years)
                if (isLeapYear && ecMonth == 12)
                {
                    ecMonthGcDay = 11;
                }
                
                DateTime ethMonthStart;
                try
                {
                    ethMonthStart = new DateTime(gregorianDate.Year, ecMonthGcMonth, ecMonthGcDay);
                }
                catch
                {
                    continue;
                }
                
                DateTime nextMonthStart;
                int nextEcMonth = (ecMonth + 1) % 13;
                int nextEcMonthGcMonth = mappedMonths[nextEcMonth];
                int nextEcMonthGcDay = monthStartDays[nextEcMonth];
                
                if (isLeapYear && nextEcMonth == 12)
                {
                    nextEcMonthGcDay = 11;
                }
                
                // Handle year crossover
                if (nextEcMonthGcMonth < ecMonthGcMonth)
                {
                    nextMonthStart = new DateTime(gregorianDate.Year + 1, nextEcMonthGcMonth, nextEcMonthGcDay);
                }
                else
                {
                    try
                    {
                        nextMonthStart = new DateTime(gregorianDate.Year, nextEcMonthGcMonth, nextEcMonthGcDay);
                    }
                    catch
                    {
                        nextMonthStart = new DateTime(gregorianDate.Year + 1, nextEcMonthGcMonth, nextEcMonthGcDay);
                    }
                }
                
                DateTime today = gregorianDate;
                
                if (today >= ethMonthStart && today < nextMonthStart)
                {
                    return ecMonth;
                }
            }
            
            // Fallback: For May 11 - June 10, return Ginbot (8)
            if (gcMonth == 5 && gcDay >= 11) return 8;
            if (gcMonth == 6 && gcDay < 11) return 8;
            
            // Default fallback to Meskerem
            return 0;
        }

        protected void GenerateCalendar()
        {
            displayedYear = inputYear;
            monthsData = new List<MonthModel>();

            bool isLeapYear = displayedYear % 4 == 0;
            int targetGregorianYear = displayedYear + 7;
            
            int[] add_nn = isLeapYear 
                ? new int[] { 12, 12, 11, 11, 10, 9, 10, 9, 9, 8, 8, 7, 6 }
                : new int[] { 11, 11, 10, 10, 9, 8, 10, 9, 9, 8, 8, 7, 6 };
            
            DateTime newYearDate = new DateTime(targetGregorianYear, 9, add_nn[0]);
            int currentWeekdayOffset = ((int)newYearDate.DayOfWeek + 6) % 7;
            DateTime runningGregorianDate = newYearDate;

            var movableHolidays = CalculateBahireHasabMovableFeasts(displayedYear);

            for (int i = 0; i < 13; i++)
            {
                int totalDaysInMonth = (i == 12) ? (isLeapYear ? 6 : 5) : 30;

                var currentMonth = new MonthModel
                {
                    AmharicName = monthNames[i].Amharic,
                    EnglishName = 
