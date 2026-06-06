using System;

public static class EthiopianCalendarConverter
{
    private static readonly string[] MonthNames =
    {
        "Meskerem", "Tikimt", "Hidar", "Tahsas", "Tir", "Yekatit",
        "Megabit", "Miazia", "Ginbot", "Sene", "Hamle", "Nehase", "Pagumē"
    };

    private const int EthiopianEpoch = 2796;

    public static (int Year, int Month, int Day, string MonthName) FromGregorian(int year, int month, int day)
    {
        long rd = GregorianToFixed(year, month, day);
        return FixedToEthiopian(rd);
    }

    private static long GregorianToFixed(int year, int month, int day)
    {
        long y = year - 1;
        bool isLeap = (year % 4 == 0) && (year % 100 != 0 || year % 400 == 0);
        int monthAdj = month <= 2 ? 0 : (isLeap ? -1 : -2);
        return 365L * y + y / 4 - y / 100 + y / 400 + (367 * month - 362) / 12 + monthAdj + day;
    }

    private static (int, int, int, string) FixedToEthiopian(long rd)
    {
        int year = (int)((4L * (rd - EthiopianEpoch) + 1463) / 1461);
        int month = (int)((rd - EthiopianToFixed(year, 1, 1)) / 30) + 1;
        int day = (int)(rd - EthiopianToFixed(year, month, 1)) + 1;
        return (year, month, day, MonthNames[month - 1]);
    }

    private static long EthiopianToFixed(int year, int month, int day) =>
        EthiopianEpoch - 1L + 365L * (year - 1) + (year - 1) / 4 + 30 * (month - 1) + day;
}
