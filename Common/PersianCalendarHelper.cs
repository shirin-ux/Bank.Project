using System.Globalization;

namespace Common
{
    public static class PersianCalendarHelper
    {
        public static DateTime ParseShamsiToGregorian(string shamsiDate)
        {
            var pc = new PersianCalendar();
            var parts = shamsiDate.Split('/', '-', '.', ' ');
            var y = int.Parse(parts[0]);
            var m = int.Parse(parts[1]);
            var d = int.Parse(parts[2]);
            return pc.ToDateTime(y, m, d, 0, 0, 0, 0);
        }
    }
}
