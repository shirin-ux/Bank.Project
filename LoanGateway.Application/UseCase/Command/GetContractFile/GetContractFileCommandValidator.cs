using FluentValidation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetContractFile;


public class GetContractFileCommandValidator : AbstractValidator<GetContractFileCommand>
{
    public GetContractFileCommandValidator()
    {
        // ProviderType
        RuleFor(x => x.ProviderType)
            .IsInEnum()
            .WithMessage("نوع بانک (ProviderType) نامعتبر است.");

        // ApprovalCode
        RuleFor(x => x.ApprovalCode)
            .GreaterThan(0)
            .WithMessage("کد تأیید (ApprovalCode) باید بزرگ‌تر از صفر باشد.");

        // NationalCode
        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کد ملی الزامی است.")
            .Length(10).WithMessage("کد ملی باید ۱۰ رقم باشد.")
            .Matches(@"^\d{10}$").WithMessage("کد ملی فقط باید شامل عدد باشد.")
            .Must(BeValidIranianNationalCode).WithMessage("کد ملی نامعتبر است.");

        // BirthDate
        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("تاریخ تولد الزامی است.")
            .Must(BeValidBirthDate)
            .WithMessage("فرمت تاریخ تولد نامعتبر است (مثال: 13660331 یا 1366/03/31).");

        // MobileNumber
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("شماره موبایل الزامی است.")
            .Length(11).WithMessage("شماره موبایل باید ۱۱ رقم باشد.")
            .Matches(@"^09\d{9}$").WithMessage("فرمت شماره موبایل نامعتبر است.");

        // PostalCode
        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("کد پستی الزامی است.")
            .Matches(@"^\d{10}$").WithMessage("کد پستی باید ۱۰ رقم عددی باشد.");

        // PhoneNumber (تلفن ثابت)
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("تلفن ثابت الزامی است.")
            .Matches(@"^\d{8,11}$")
            .WithMessage("تلفن ثابت باید بین ۸ تا ۱۱ رقم و فقط عدد باشد.");


        RuleFor(x => x.LoanAmount)
            .GreaterThan(0).WithMessage("مبلغ وام باید بزرگ‌تر از صفر باشد.");

        // InstallmentCount
        RuleFor(x => x.InstallmentCount)
            .GreaterThan((short)0)
            .WithMessage("تعداد اقساط باید بزرگ‌تر از صفر باشد.");

        // Address (اختیاری، فقط محدودیت طول)
        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Address))
            .WithMessage("آدرس حداکثر می‌تواند ۵۰۰ کاراکتر باشد.");

        // CbTrackingCode (اختیاری، فقط عددی بودن)
        RuleFor(x => x.CbTrackingCode)
            .Matches(@"^\d+$")
            .When(x => !string.IsNullOrWhiteSpace(x.CbTrackingCode))
            .WithMessage("کد رهگیری بانک مرکزی فقط باید شامل عدد باشد.");
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
            "5555555555","6666666665","7777777777","8888888888","9999999999"
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
            return false;

        return DateTime.TryParseExact(
            birthDate,
            new[] {"yyyy/MM/dd" },
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }
}