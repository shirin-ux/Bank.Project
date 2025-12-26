using LoanGateway.Auth.Domain.Enum;
using LoanService.Domain.Exceptions;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

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
        public static string GenerateOtpCode(int length)
        {
            var random = RandomNumberGenerator.GetInt32((int)Math.Pow(10, length - 1),
                                                        (int)Math.Pow(10, length));
            return random.ToString(CultureInfo.InvariantCulture);
        }

        public static byte[] Hash(string code, string phoneNumber, OtpPurpose purpose)
        {

            var input = $"{code}|{phoneNumber}|{(byte)purpose}";
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        }
        public static byte[] Hash(string token)
        {

            var tokenBytes = Convert.FromBase64String(token);

            using var sha = SHA256.Create();
            return sha.ComputeHash(tokenBytes);
        }

        public static bool Verify(string code, string phoneNumber, OtpPurpose purpose, byte[] storedHash)
        {
            var newHash = Hash(code, phoneNumber, purpose);
            return ConstantTimeEquals(newHash, storedHash);
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            var diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
        public static DateTime ConvertPersianToGregorian(string persianDate)
        {
            var parts = persianDate.Split('/');
            if (parts.Length != 3)
                throw new ArgumentException("فرمت تاریخ تولد نامعتبر است");

            var pc = new PersianCalendar();

            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

    }

}
