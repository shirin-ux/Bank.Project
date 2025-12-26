using System.Globalization;

namespace LoanService.Domain.ValueObjects
{
    /// <summary>
    /// /اطلاعات مشتری مثل کد ملی، موبایل،
    /// </summary>
    /// <param name="NationalCode"></param>
    /// <param name="BirthDate"></param>
    /// <param name="Mobile"></param>
    /// <param name="PostalCode"></param>
    /// <param name="Gender"></param>
    public sealed record CustomerInfo
    {
        public string NationalCode { get; }
        public DateTime? BirthDate { get; }
        public string? Mobile { get; }
        public string? PostalCode { get; }
        public string? Gender { get; }


        public CustomerInfo(string nationalCode, string birthDate, string mobile, string postalCode, string gender)
        {
            if (string.IsNullOrWhiteSpace(nationalCode))
                throw new Exception("کد ملی نمی‌تواند خالی باشد.");

            BirthDate = ParseShamsiToGregorian(birthDate);

            NationalCode = nationalCode;
            Mobile = mobile;
            PostalCode = postalCode;
            Gender = gender;
        }


        public CustomerInfo(string nationalCode, DateTime? birthDate, string mobile, string postalCode, string gender)
        {
            NationalCode = nationalCode;
            BirthDate = birthDate;
            Mobile = mobile;
            PostalCode = postalCode;
            Gender = gender;
        }

        private static DateTime ParseShamsiToGregorian(string shamsiDate)
        {
            var pc = new PersianCalendar();
            var parts = shamsiDate.Split('/', '-', '.', ' ');
            var y = int.Parse(parts[0]);
            var m = int.Parse(parts[1]);
            var d = int.Parse(parts[2]);
            return pc.ToDateTime(y, m, d, 0, 0, 0, 0);
        }
    }

    //public sealed record CustomerInfo
    //{
    //    public string? NationalCode { get; }
    //    public DateTime? BirthDate { get; } 
    //    public string? Mobile { get; }
    //    public string? PostalCode { get; }
    //    public string? Gender { get; }

    //    public CustomerInfo(string nationalCode, string birthDate, string mobile, string postalCode, string gender)
    //    {
    //        if (string.IsNullOrWhiteSpace(nationalCode))
    //            throw new Exception("کد ملی نمی‌تواند خالی باشد.");

    //        BirthDate = ToGregorianDateOnly(birthDate);

    //        NationalCode = nationalCode;
    //        Mobile = mobile;
    //        PostalCode = postalCode;
    //        Gender = gender;
    //    }


    //    public CustomerInfo(string nationalCode, DateTime? birthDate, string mobile, string postalCode, string gender)
    //    {
    //        NationalCode = nationalCode;
    //        BirthDate = birthDate;
    //        Mobile = mobile;
    //        PostalCode = postalCode;
    //        Gender = gender;
    //    }

    //    private static DateTime ParseShamsiToGregorian(string shamsiDate)
    //    {
    //        var pc = new PersianCalendar();
    //        var parts = shamsiDate.Split('/', '-', '.', ' ');
    //        var y = int.Parse(parts[0]);
    //        var m = int.Parse(parts[1]);
    //        var d = int.Parse(parts[2]);
    //        return pc.ToDateTime(y, m, d, 0, 0, 0, 0);
    //    }

    //    private static DateTime ToGregorianDateOnly(string shamsiDate)
    //    {
    //        if (string.IsNullOrWhiteSpace(shamsiDate))
    //            throw new Exception("تاریخ تولد وارد نشده است.");

    //        var pc = new PersianCalendar();

    //        // انتظار فرمت: yyyy/MM/dd
    //        var parts = shamsiDate.Split('/', '-', '.', ' ');
    //        if (parts.Length != 3)
    //            throw new Exception("فرمت تاریخ تولد معتبر نیست. مثال: 1380/05/21");

    //        int y = int.Parse(parts[0]);
    //        int m = int.Parse(parts[1]);
    //        int d = int.Parse(parts[2]);

    //        try
    //        {
    //            var gregorian = pc.ToDateTime(y, m, d, 0, 0, 0, 0);
    //            return gregorian;
    //        }
    //        catch
    //        {
    //            throw new Exception("تاریخ تولد معتبر نیست.");
    //        }
    //    }
    //}

}
