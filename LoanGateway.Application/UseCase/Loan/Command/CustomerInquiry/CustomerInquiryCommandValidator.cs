using FluentValidation;
using System.Text.RegularExpressions;

namespace LoanService.Application.UseCase.Loan.Command.CustomerInquiry;

public class CustomerInquiryCommandValidator : AbstractValidator<CustomerInquiryCommand>
{
    public CustomerInquiryCommandValidator()
    {
   
        RuleFor(x => x.ProviderType)
            .IsInEnum()
            .WithMessage("نوع تأمین‌کننده مالی نامعتبر است.");

        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کد ملی الزامی است.")
            .Length(10).WithMessage("کد ملی باید ۱۰ رقم باشد.")
            .Matches(@"^\d{10}$").WithMessage("کد ملی فقط باید شامل عدد باشد.")
            .Must(BeValidIranianNationalCode).WithMessage("کد ملی نامعتبر است.");


        RuleFor(x => x.ConfigType)
            .Must(ct => ct is null or 1 or 2 or 3)
            .WithMessage("ConfigType در صورت ارسال فقط می‌تواند یکی از مقادیر 1، 2 یا 3 باشد.");

        //RuleFor(x => x.BirthDate)
        //    .Must(BeValidBirthDate)
        //    .When(x => !string.IsNullOrWhiteSpace(x.BirthDate))
        //    .WithMessage("فرمت تاریخ تولد نامعتبر است (مثال: 13700101).");


        RuleFor(x => x.MobileNo)
            .NotEmpty().WithMessage("شماره موبایل الزامی است.")
            .Length(11).WithMessage("شماره موبایل باید ۱۱ رقم باشد.")
            .Matches(@"^09\d{9}$").WithMessage("فرمت شماره موبایل نامعتبر است.");

        RuleFor(x => x.PostalCode)
            .Matches(@"^\d{10}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode))
            .WithMessage("کد پستی باید ۱۰ رقم باشد.");

  
        RuleFor(x => x.RequestAmount)
            .GreaterThan(0)
            .When(x => x.RequestAmount.HasValue)
            .WithMessage("مبلغ درخواست باید بزرگ‌تر از صفر باشد.");


        //RuleFor(x => x.ApprovalCode)
        //    .GreaterThan(0)
        //    .When(x => x.ApprovalCode.HasValue)
        //    .WithMessage("کد تأیید باید بزرگ‌تر از صفر باشد.");


        //RuleFor(x => x.CbTrackingCode)
        //    .GreaterThan(0)
        //    .When(x => x.CbTrackingCode.HasValue)
        //    .WithMessage("کد رهگیری بانک مرکزی باید بزرگ‌تر از صفر باشد.");
    }

    private bool BeValidIranianNationalCode(string nationalCode)
    {
        if (string.IsNullOrWhiteSpace(nationalCode) || nationalCode.Length != 10)
            return false;

        if (!Regex.IsMatch(nationalCode, @"^\d{10}$"))
            return false;


        var invalids = new[]
        {
            "0000000000","1111111111","2222222222","3333333333","4444444444",
            "5555555555","6666666666","7777777777","8888888888","9999999999"
        };
        if (invalids.Contains(nationalCode))
            return false;


        var check = nationalCode[9] - '0';
        var sum = 0;

        for (int i = 0; i < 9; i++)
            sum += (nationalCode[i] - '0') * (10 - i);

        var remainder = sum % 11;

        return (remainder < 2 && check == remainder) ||
               (remainder >= 2 && check == 11 - remainder);
    }

    private bool BeValidBirthDate(string? birthDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate))
            return true;


        if (!Regex.IsMatch(birthDate, @"^\d{8}$"))
            return false;


        var year = int.Parse(birthDate.Substring(0, 4));
        var month = int.Parse(birthDate.Substring(4, 2));
        var day = int.Parse(birthDate.Substring(6, 2));

        if (month is < 1 or > 12) return false;
        if (day is < 1 or > 31) return false;

        return true;
    }
}

