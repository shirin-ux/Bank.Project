using System.Globalization;

namespace Common
{
    public static class DateConversion
    {
        public static int ToBankIntDate(string? ddMmYyyy)
        {
            if (string.IsNullOrWhiteSpace(ddMmYyyy))
                throw new ArgumentException("تاریخ نامعتبر است.", nameof(ddMmYyyy));

            if (!DateTime.TryParseExact(ddMmYyyy.Trim(), "YYYY/MM/DD", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                throw new FormatException($"فرمت تاریخ باید 'DD/MM/YYYY' باشد. مقدار دریافتی: '{ddMmYyyy}'");

            return dt.Year * 10000 + dt.Month * 100 + dt.Day; // YYYYMMDD as int
        }
    }
}
