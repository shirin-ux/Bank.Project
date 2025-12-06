using LoanService.Domain.Exceptions;
using System.Globalization;

namespace Common
{
 

    public static class DateExtensions
    {
        public static string ToKarizmahBirthDate(this string persianBirthDate)
        {
            if (string.IsNullOrWhiteSpace(persianBirthDate))
                throw new LogicException("تاریخ تولد کاربر خالی است.");


            var parts = persianBirthDate.Trim().Split('/', '-');
            if (parts.Length != 3)
                throw new LogicException("فرمت تاریخ تولد نامعتبر است. فرمت صحیح: 1370/01/02");

            if (!int.TryParse(parts[0], out var y) ||
                !int.TryParse(parts[1], out var m) ||
                !int.TryParse(parts[2], out var d))
                throw new LogicException("فرمت تاریخ تولد نامعتبر است.");

            var pc = new PersianCalendar();
            var gregorian = pc.ToDateTime(y, m, d, 0, 0, 0, 0);

            return gregorian.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }

}
