using System.Collections.Generic;

namespace EthiopianCalendarPortal.Pages
{
    public class MonthModel
    {
        public string AmharicName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public int StartWeekday { get; set; }
        // ADDED PROPERTY: Tracks the matching Gregorian year for this specific month card
        public string GregorianYearText { get; set; } = string.Empty;
        public List<DayModel> Days { get; set; } = new List<DayModel>();
    }

    public class DayModel
    {
        public int EthiopianDay { get; set; }
        public string GregorianDateText { get; set; } = string.Empty;
        public string HolidayName { get; set; } = string.Empty;
        public string BgClass { get; set; } = string.Empty;
    }
}
