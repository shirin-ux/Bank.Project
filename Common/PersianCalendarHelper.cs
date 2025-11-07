using System.Globalization;

namespace Common
{
    public static class PersianCalendarHelper
    {
        private static DateOnly ToGregorianDateOnly(string shamsiDate)
        {
            if (string.IsNullOrWhiteSpace(shamsiDate))
                throw new Exception("تاریخ تولد وارد نشده است.");

            var pc = new PersianCalendar();

            // انتظار فرمت: yyyy/MM/dd
            var parts = shamsiDate.Split('/', '-', '.', ' ');
            if (parts.Length != 3)
                throw new Exception("فرمت تاریخ تولد معتبر نیست. مثال: 1380/05/21");

            int y = int.Parse(parts[0]);
            int m = int.Parse(parts[1]);
            int d = int.Parse(parts[2]);

            try
            {
                var gregorian = pc.ToDateTime(y, m, d, 0, 0, 0, 0);
                return DateOnly.FromDateTime(gregorian);
            }
            catch
            {
                throw new Exception("تاریخ تولد معتبر نیست.");
            }
        }
    }
}
