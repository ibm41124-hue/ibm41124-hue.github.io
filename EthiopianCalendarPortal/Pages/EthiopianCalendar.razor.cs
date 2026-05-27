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

        protected readonly string[] weekdayHeaders = { "ሰኞ", "ማክሰኞ", "ረቡዕ", "ሐሙስ", "አርብ", "ቅዳሜ", "እሁድ" };

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
            // Set inputYear to today's Ethiopian year
            DateTime today = DateTime.UtcNow;
            inputYear = ConvertGregorianToEthiopianYear(today);
            GenerateCalendar();
            
            // Set currentMonthIndex to today's month (Ginbot = index 8 for May 2026)
            currentMonthIndex = ConvertGregorianMonthToEthiopianMonth(today.Month, today.Day, inputYear);
        }

        // Convert Gregorian date to Ethiopian year
        private int ConvertGregorianToEthiopianYear(DateTime gregorianDate)
        {
            int gcYear = gregorianDate.Year;
            int ecYear = gcYear - 7;
            
            // Ethiopian New Year is Sept 11 (or Sept 12 in leap years)
            // If before New Year, subtract 1 more year
            if (gregorianDate.Month < 9 || (gregorianDate.Month == 9 && gregorianDate.Day < 11))
            {
                ecYear--;
            }
            
            return ecYear;
        }

        // Convert Gregorian month/day to Ethiopian month index (0-12)
        private int ConvertGregorianMonthToEthiopianMonth(int gcMonth, int gcDay, int ecYear)
        {
            bool isLeapYear = ecYear % 4 == 0;
            
            // Ethiopian month start dates in Gregorian (for non-leap year)
            int[] gcMonths = { 9, 9, 10, 10, 11, 11, 12, 12, 1, 2, 3, 4, 5 };
            int[] gcDays = { 11, 27, 26, 25, 25, 24, 24, 23, 23, 22, 22, 21, 20 };
            
            // Adjust for leap year
            if (isLeapYear && gcMonth >= 2 && gcMonth <= 9)
            {
                for (int i = 0; i < gcMonths.Length; i++)
                {
                    if (gcMonths[i] >= 2 && gcMonths[i] <= 9)
                    {
                        gcDays[i]++;
                    }
                }
            }
            
            DateTime today = new DateTime(DateTime.UtcNow.Year, gcMonth, gcDay);
            
            for (int i = 0; i < 13; i++)
            {
                int nextI = (i + 1) % 13;
                DateTime start = new DateTime(DateTime.UtcNow.Year, gcMonths[i], gcDays[i]);
                DateTime end = new DateTime(DateTime.UtcNow.Year, gcMonths[nextI], gcDays[nextI]).AddDays(-1);
                
                // Handle year crossover
                if (gcMonths[i] > gcMonths[nextI])
                {
                    end = new DateTime(DateTime.UtcNow.Year + 1, gcMonths[nextI], gcDays[nextI]).AddDays(-1);
                }
                
                if (today >= start && today <= end)
                {
                    return i;
                }
            }
            
            // Fallback: return Ginbot (8) for May
            return 8;
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
                    EnglishName = monthNames[i].English,
                    StartWeekday = currentWeekdayOffset,
                    GregorianYearText = runningGregorianDate.ToString("yyyy"),
                    Days = new List<DayModel>()
                };

                for (int day = 1; day <= totalDaysInMonth; day++)
                {
                    var dayModel = new DayModel 
                    { 
                        EthiopianDay = day, 
                        GregorianDateText = runningGregorianDate.ToString("MMM d, yyyy") 
                    };

                    AssignFixedFeasts(i + 1, day, dayModel);

                    int absoluteDayOfYear = (i * 30) + day;
                    if (movableHolidays.TryGetValue(absoluteDayOfYear, out string? movableName))
                    {
                        dayModel.HolidayName = movableName;
                        dayModel.BgClass = movableName.Contains("ትንሣኤ") || movableName.Contains("ልደት")
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

        protected void PrevMonth()
        {
            if (currentMonthIndex > 0) currentMonthIndex--;
        }

        protected void NextMonth()
        {
            if (monthsData != null && currentMonthIndex < monthsData.Count - 1)
                currentMonthIndex++;
        }

        private Dictionary<int, string> CalculateBahireHasabMovableFeasts(int ethiopianYear)
        {
            var holidays = new Dictionary<int, string>();
            int ameteAlem = ethiopianYear + 5500;

            int medeb = ameteAlem % 19;
            int wenber = medeb == 0 ? 18 : (medeb == 1 ? 0 : medeb - 1);
            int abekte = (11 * wenber) % 30;
            int metq = Math.Abs(30 - abekte);
            int metqMonth = metq > 14 ? 1 : 2;
            
            int meteneRabit = ameteAlem / 4;
            int mebacha = (int)((ameteAlem + meteneRabit) % 7);
            
            int tewsakDay = WeekdayN(metqMonth, metq, mebacha);
            
            int tewsak = tewsakDay switch
            {
                6 => 127,
                0 => 126,
                1 => 125,
                2 => 124,
                3 => 123,
                4 => 122,
                5 => 128,
                _ => 0
            };
            
            var (ninivehMonth, ninivehDay) = Monthday(metqMonth, metq, tewsak);
            
            holidays[((ninivehMonth - 1) * 30) + ninivehDay] = "ጾመ ነነዌ (Nineveh Fast)";
            
            var (lentMonth, lentDay) = Monthday(ninivehMonth, ninivehDay, 14);
            holidays[((lentMonth - 1) * 30) + lentDay] = "ዐቢይ ጾም (Great Lent)";
            
            var (debreMonth, debreDay) = Monthday(lentMonth, lentDay, 27);
            holidays[((debreMonth - 1) * 30) + debreDay] = "ደብረ ዘይት (Mount of Olives)";
            
            var (fridayMonth, fridayDay) = Monthday(debreMonth, debreDay, 26);
            holidays[((fridayMonth - 1) * 30) + fridayDay] = "ስቅለት (Good Friday)";
            
            var (easterMonth, easterDay) = Monthday(debreMonth, debreDay, 28);
            int easterAbs = ((easterMonth - 1) * 30) + easterDay;
            holidays[easterAbs] = "ትንሣኤ (Easter)";
            
            var (rkMonth, rkDay) = Monthday(easterMonth, easterDay, 24);
            holidays[((rkMonth - 1) * 30) + rkDay] = "ርክበ ካህናት (Assembly of Priests)";
            
            var (erigetMonth, erigetDay) = Monthday(easterMonth, easterDay, 39);
            holidays[((erigetMonth - 1) * 30) + erigetDay] = "ዕርገት (Ascension)";
            
            var (peraklitosMonth, peraklitosDay) = Monthday(easterMonth, easterDay, 49);
            holidays[((peraklitosMonth - 1) * 30) + peraklitosDay] = "ጰራቅሊጦስ (Pentecost/Peraklitos)";
            
            var (hawariyatMonth, hawariyatDay) = Monthday(easterMonth, easterDay, 50);
            holidays[((hawariyatMonth - 1) * 30) + hawariyatDay] = "ጾመ ሐዋርያት (Apostles Fast)";

            return holidays;
        }

        private int WeekdayN(int m, int dy, int mebacha)
        {
            return (((m - 1) * 30 + (dy - 1)) % 7 + mebacha) % 7;
        }

        private (int month, int day) Monthday(int m1, int d1, int x)
        {
            int d2 = (d1 + x) % 30;
            int m2 = m1 + (d1 + x) / 30;
            if (d2 == 0)
            {
                d2 = 30;
                m2 = m2 - 1;
            }
            return (m2, d2);
        }

        private void AssignFixedFeasts(int month, int day, DayModel model)
        {
            if (day == 5) { model.HolidayName = "አቡነ ገብረ መንፈስ ቅዱስ"; model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 7) { model.HolidayName = "ሥላሴ (Holy Trinity)"; model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 12) { model.HolidayName = "ቅዱስ ሚካኤል (St. Michael)"; model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 16) { model.HolidayName = "ቅድስት ኪዳነ ምሕረት (Kidane Mihret)"; model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 19) { model.HolidayName = "ቅዱስ ገብርኤል (St. Gabriel)"; model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 21) { model.HolidayName = "ቅድስት ማርያም (St. Mary)"; model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 23) { model.HolidayName = "ቅዱስ ጊዮርጊስ (St. George)"; model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 27) { model.HolidayName = "መድኃኔዓለም (Savior of the World)"; model.BgClass = "bg-info text-dark opacity-75"; }
            else if (day == 29) { model.HolidayName = "በዓለ ወልድ (Feast of the Son)"; model.BgClass = "bg-info text-dark opacity-75"; }

            if (month == 1 && day == 1) { model.HolidayName = "እንቁጣጣሽ / New Year"; model.BgClass = "bg-warning fw-bold text-dark"; }
            else if (month == 1 && day == 17) { model.HolidayName = "መስቀል / Meskel"; model.BgClass = "bg-warning fw-bold text-dark"; }
            else if (month == 4 && day == 29) { model.HolidayName = "ልደት / Christmas (Genna)"; model.BgClass = "bg-warning fw-bold text-dark"; }
            else if (month == 5 && day == 11) { model.HolidayName = "ጥምቀት / Timkat (Epiphany)"; model.BgClass = "bg-warning fw-bold text-dark"; }
            else if (month == 5 && day == 12) { model.HolidayName = "ቃና ዘገሊላ / Kana ZeGelila"; model.BgClass = "bg-warning fw-bold text-dark"; }
            else if (month == 12 && day == 13) { model.HolidayName = "ደብረ ታቦር / Buhe"; model.BgClass = "bg-warning fw-bold text-dark"; }
        }

        protected string GetTypeClass(string? bgClass)
        {
            if (string.IsNullOrEmpty(bgClass)) return "";
            if (bgClass.Contains("warning")) return "is-annual";
            if (bgClass.Contains("danger")) return "is-movable";
            if (bgClass.Contains("info")) return "is-monthly";
            return "";
        }

        protected string GetDotClass(string? bgClass)
        {
            if (string.IsNullOrEmpty(bgClass)) return "";
            if (bgClass.Contains("warning")) return "annual";
            if (bgClass.Contains("danger")) return "movable";
            if (bgClass.Contains("info")) return "monthly";
            return "";
        }
    }
}
