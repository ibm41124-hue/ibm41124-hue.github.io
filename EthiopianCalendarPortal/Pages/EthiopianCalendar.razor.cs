using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace EthiopianCalendarPortal.Pages
{
    public partial class EthiopianCalendar : ComponentBase
    {
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

        protected override void OnInitialized()
        {
            GenerateCalendar();
        }

        protected void GenerateCalendar()
        {
            displayedYear = inputYear;
            monthsData = new List<MonthModel>();

            // FIXED: Ethiopian leap year occurs when year % 4 == 3
            bool isLeapYear = displayedYear % 4 == 3;
            
            // FIXED: Ethiopian New Year is always September 11 (or 12 in leap year before Gregorian leap)
            int targetGregorianYear = displayedYear + 7;
            DateTime newYearDate = new DateTime(targetGregorianYear, 9, 11);
            int currentWeekdayOffset = (int)newYearDate.DayOfWeek;

            var movableHolidays = CalculateBahireHasabMovableFeasts(displayedYear);

            DateTime runningGregorianDate = newYearDate;

            for (int i = 0; i < 13; i++)
            {
                int totalDaysInMonth = (i == 12) ? (isLeapYear ? 6 : 5) : 30;

                var currentMonth = new MonthModel
                {
                    AmharicName = monthNames[i].Amharic,
                    EnglishName = monthNames[i].English,
                    StartWeekday = currentWeekdayOffset,
                    GregorianYearText = runningGregorianDate.ToString("yyyy"),
                    Days = new List<DayModel>()
                };

                for (int day = 1; day <= totalDaysInMonth; day++)
                {
                    string dateText = runningGregorianDate.ToString("MMM d");
                    var dayModel = new DayModel { EthiopianDay = day, GregorianDateText = dateText };

                    AssignFixedFeasts(i + 1, day, dayModel, isLeapYear);

                    int absoluteDayOfYear = (i * 30) + day;
                    if (movableHolidays.TryGetValue(absoluteDayOfYear, out string? movableName))
                    {
                        dayModel.HolidayName = movableName;
                        dayModel.BgClass = (movableName.Contains("ትንሣኤ") || movableName.Contains("ልደት"))
                            ? "bg-warning fw-bold text-dark"
                            : "bg-danger text-white fw-bold";
                    }

                    currentMonth.Days.Add(dayModel);
                    runningGregorianDate = runningGregorianDate.AddDays(1);
                }

                monthsData.Add(currentMonth);
                currentWeekdayOffset = (currentWeekdayOffset + totalDaysInMonth) % 7;
            }
        }

        // REMOVED: GetFirstDayOfMäskärämWeekday - no longer needed, using correct New Year date directly

        private Dictionary<int, string> CalculateBahireHasabMovableFeasts(int ethiopianYear)
        {
            var holidays = new Dictionary<int, string>();
            int ameteAlem = ethiopianYear + 5500;

            int goldenNumber = (ameteAlem % 19) + 1;
            int abekte = (goldenNumber * 11) % 30;
            int metqi = abekte == 0 ? 30 : 30 - abekte;

            int metqiMonth = (abekte <= 14) ? 1 : 2;
            int metqiDay = metqi;

            int metqiAbsoluteDays = ((metqiMonth - 1) * 30) + metqiDay;
            
            // FIXED: Use the correct starting weekday from New Year date
            int targetGregorianYear = ethiopianYear + 7;
            DateTime newYearDate = new DateTime(targetGregorianYear, 9, 11);
            int startingWeekday = (int)newYearDate.DayOfWeek;
            
            int metqiWeekday = (startingWeekday + metqiAbsoluteDays - 1) % 7;

            int[] weekdayTewsak = { 7, 6, 5, 4, 3, 2, 8 };
            int tewsak = weekdayTewsak[metqiWeekday];

            int ninivehDay;
            int ninivehMonth;

            if (metqiMonth == 1 && (metqiDay + tewsak) > 30)
            {
                ninivehMonth = 5;
                ninivehDay = (metqiDay + tewsak) - 30;
            }
            else if (metqiMonth == 1)
            {
                ninivehMonth = 5;
                ninivehDay = metqiDay + tewsak;
            }
            else
            {
                ninivehMonth = 6;
                ninivehDay = (metqiDay + tewsak > 30) ? (metqiDay + tewsak) - 30 : metqiDay + tewsak;
            }

            int ninivehAbsolute = ((ninivehMonth - 1) * 30) + ninivehDay;

            holidays[ninivehAbsolute]       = "ጾመ ነነዌ (Nineveh Fast)";
            holidays[ninivehAbsolute + 14]  = "ዐቢይ ጾም (Great Lent)";
            holidays[ninivehAbsolute + 41]  = "ደብረ ዘይት (Mount of Olives)";
            holidays[ninivehAbsolute + 62]  = "ሆሣዕና (Palm Sunday)";
            holidays[ninivehAbsolute + 67]  = "ስቅለት (Good Friday)";
            holidays[ninivehAbsolute + 69]  = "ትንሣኤ (Easter)";
            holidays[ninivehAbsolute + 93]  = "ርክበ ካህናት (Assembly of Priests)";
            holidays[ninivehAbsolute + 109] = "ዕርገት (Ascension)";
            holidays[ninivehAbsolute + 119] = "ጰራቅሊጦስ (Pentecost)";
            holidays[ninivehAbsolute + 121] = "ጾመ ሐዋርያት (Apostles Fast)";

            return holidays;
        }

        private void AssignFixedFeasts(int month, int day, DayModel model, bool isLeapYear)
        {
            // Monthly recurring saint days (set first; annual feasts below will override where needed)
            if      (day == 5)  { model.HolidayName = "አቡነ ገብረ መንፈስ ቅዱስ";                          model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 7)  { model.HolidayName = "ሥላሴ (Holy Trinity)";                          model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 12) { model.HolidayName = "ቅዱስ ሚካኤል (St. Michael)";                     model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 16) { model.HolidayName = "ቅድስት ኪዳነ ምሕረት (Kidane Mihret)";             model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 19) { model.HolidayName = "ቅዱስ ገብርኤል (St. Gabriel)";                   model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 21) { model.HolidayName = "ቅድስት ማርያም (St. Mary)";                      model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 23) { model.HolidayName = "ቅዱስ ጊዮርጊስ (St. George)";                   model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 27) { model.HolidayName = "መድኃኔዓለም (Savior of the World)";             model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 29) { model.HolidayName = "በዓለ ወልድ (Feast of the Son)";                model.BgClass = "bg-info text-dark opacity-75"; }

            // Annual fixed feasts (override monthly saint days on these specific dates)
            if      (month == 1  && day == 1)  { model.HolidayName = "እንቁጣጣሽ / ቅዱስ ዮሐንስ (New Year)";              model.BgClass = "bg-warning text-dark fw-bold"; }
            else if (month == 1  && day == 17) { model.HolidayName = "መስቀል (Feast of the True Cross)";               model.BgClass = "bg-warning text-dark fw-bold"; }
            else if (month == 4  && day == 29) { model.HolidayName = "ልደት (Christmas)";                              model.BgClass = "bg-warning text-dark fw-bold"; }
            else if (month == 5  && day == 11) { model.HolidayName = "ጥምቀት (Epiphany)";                              model.BgClass = "bg-warning text-dark fw-bold"; }
            else if (month == 5  && day == 12) { model.HolidayName = "ቃና ዘገሊላ (Kana ZeGelila)";                     model.BgClass = "bg-warning text-dark fw-bold"; }
            else if (month == 12 && day == 13) { model.HolidayName = "ደብረ ታቦር (Debre Tabor / Buhe)";               model.BgClass = "bg-warning text-dark fw-bold"; }
        }
    }
}
