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

        protected override void OnInitialized() => GenerateCalendar();

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
