namespace EthiopianCalendarPortal.Helpers;

public static class AmharicTranslator
{
    private static readonly Dictionary<string, string> Days = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Sunday",     "እሑድ"  },
        { "Monday",     "ሰኞ"   },
        { "Tuesday",    "ማክሰኞ" },
        { "Wednsday",  "ረቡዕ"  },
        { "Thursday",   "ሐሙስ"  },
        { "Friday",     "ዓርብ"  },
        { "Saturday",   "ቅዳሜ"  },
    };

    private static readonly Dictionary<string, string> Months = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Meskerem",  "መስከረም" },
        { "Tikemt",    "ጥቅምት"  },
        { "Hidar",     "ኅዳር"   },
        { "Tahsas",    "ታኅሣሥ"  },
        { "Tahisas",   "ታኅሣሥ"  },
        { "Tir",       "ጥር"    },
        { "Yekatit",   "የካቲት"  },
        { "Megabit",   "መጋቢት"  },
        { "Miyazya",   "ሚያዝያ"  },
        { "Miazia",    "ሚያዝያ"  },
        { "Ginbot",    "ግንቦት"  },
        { "Sene",      "ሰኔ"    },
        { "Hamle",     "ሐምሌ"   },
        { "Nehase",    "ነሐሴ"   },
        { "Pagume",    "ጳጉሜ"   },
    };

    private static readonly Dictionary<string, string> Holidays = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Tahisas",                    "ታኅሣሥ"                     },
        { "Tahsas",                     "ታኅሣሥ"                     },
        { "Miazia",                     "ሚያዝያ"                     },
        { "Miyazya",                    "ሚያዝያ"                     },
        { "Adis Amet",                  "አዲስ አመት"                  },
        { "Meskel",                     "መስቀል"                      },
        { "Tsome Nebiyat",              "ጾመ ነቢያት"                  },
        { "Lidet",                      "ልደተ ክርስቶስ"                },
        { "Gena",                       "ልደተ ክርስቶስ"                },
        { "Tsome Gahad",                "ጾመ ጋድ"                    },
        { "Timket",                     "ጥምቀት"                      },
        { "Tsome Nenewe",               "ጾመ ሰብአ ነነዌ"               },
        { "Tsome Nenewe Mefcha",        "ጾመ ነነዌ መፍቻ"              },
        { "Tsome Hudade",               "ዐቢይ ጾም"                   },
        { "Debre Zeyit",                "ደብረ ዘይት"                  },
        { "Siklet",                     "ስቅለት"                      },
        { "Fasika",                     "ትንሣኤ"                      },
        { "Rikbe Kahinat",              "ርክበ ካህናት"                 },
        { "Eriget",                     "ዕርገት"                      },
        { "Peraklitos",                 "ጰራቅሊጦስ"                  },
        { "Tsome Hawariyat",            "ጾመ ሐዋርያት"                 },
        { "Tsome Dehinet",              "ጾመ ድኅነት"                  },
        { "Tsome Hawariyat Mefcha",     "ጾመ ሐዋርያት መፍቻ"           },
        { "Tsome Filseta Maryam",       "ጾመ ፍልሰታ ለማርያም"          },
        { "Debre Tabor",                "ደብረ ታቦር"                  },
        { "Tsome Filseta Mefcha",       "ጾመ ፍልሰታ መፍቻ"            },
    };

    public static string ToAmharicDay(string day)
    {
        if (string.IsNullOrWhiteSpace(day)) return day;
        if (Days.TryGetValue(day.Trim(), out var exact)) return exact;
        foreach (var pair in Days)
            if (day.Trim().StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        return day;
    }

    public static string ToAmharicMonth(string month)
    {
        if (string.IsNullOrWhiteSpace(month)) return month;
        if (Months.TryGetValue(month.Trim(), out var exact)) return exact;
        foreach (var pair in Months)
            if (month.Trim().StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        return month;
    }

    public static string ToAmharicHoliday(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name;
        if (Holidays.TryGetValue(name.Trim(), out var exact)) return exact;
        foreach (var pair in Holidays)
            if (name.Trim().StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
                return pair.Value;
        return name;
    }
}
