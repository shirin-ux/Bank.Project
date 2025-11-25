using FluentValidation;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LoanService.Application.UseCase.Loan.Command.GetCollateralContractFile;
public class GetCollateralContractFileValidator : AbstractValidator<GetCollateralContractFileCommand>
{
    public GetCollateralContractFileValidator()
    {
      
        RuleFor(x => x.ProviderType)
            .IsInEnum()
            .WithMessage("نوع بانک (ProviderType) نامعتبر است.");

  
        RuleFor(x => x.ApprovalCode)
            .GreaterThan(0)
            .WithMessage("کد تأیید (ApprovalCode) باید بزرگ‌تر از صفر باشد.");

 
        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کد ملی الزامی است.")
            .Length(10).WithMessage("کد ملی باید ۱۰ رقم باشد.")
            .Matches(@"^\d{10}$").WithMessage("کد ملی فقط باید شامل عدد باشد.")
            .Must(BeValidIranianNationalCode).WithMessage("کد ملی نامعتبر است.");

        RuleFor(x => x.GuarantorNC)
            .NotEmpty().WithMessage("کد ملی ضامن الزامی است.")
            .Length(10).WithMessage("کد ملی ضامن باید ۱۰ رقم باشد.")
            .Matches(@"^\d{10}$").WithMessage("کد ملی ضامن فقط باید شامل عدد باشد.")
            .Must(BeValidIranianNationalCode).WithMessage("کد ملی ضامن نامعتبر است.");

   
        RuleFor(x => x.CollateralDate)
            .NotEmpty().WithMessage("تاریخ وثیقه الزامی است.")
            .Must(BeValidDate)
            .WithMessage("فرمت تاریخ وثیقه نامعتبر است (مثال: 14030915 یا 1403/09/15).");

    
        RuleFor(x => x.CollateralAmount)
            .GreaterThan(0)
            .WithMessage("مبلغ وثیقه باید بزرگ‌تر از صفر باشد.");

        
        RuleFor(x => x.CollateralNo)
            .GreaterThan(0)
            .WithMessage("شماره وثیقه باید بزرگ‌تر از صفر باشد.");

      
        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("تاریخ تولد الزامی است.")
            .Must(BeValidDate)
            .WithMessage("فرمت تاریخ تولد نامعتبر است  1366/03/31.");

    
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("شماره موبایل الزامی است.")
            .Length(11).WithMessage("شماره موبایل باید ۱۱ رقم باشد.")
            .Matches(@"^09\d{9}$").WithMessage("فرمت شماره موبایل نامعتبر است.");

       
        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("کد پستی الزامی است.")
            .Matches(@"^\d{10}$").WithMessage("کد پستی باید ۱۰ رقم عددی باشد.");

        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("تلفن ثابت الزامی است.")
            .Matches(@"^\d{8,11}$")
            .WithMessage("تلفن ثابت باید بین ۸ تا ۱۱ رقم و فقط عدد باشد.");

 
        RuleFor(x => x.LoanAmount)
            .GreaterThan(0)
            .When(x => x.LoanAmount.HasValue)
            .WithMessage("مبلغ وام باید در صورت ارسال، بزرگ‌تر از صفر باشد.");


        RuleFor(x => x.InstallmentCount)
            .GreaterThan((short)0)
            .WithMessage("تعداد اقساط باید بزرگ‌تر از صفر باشد.");


        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Address))
            .WithMessage("آدرس حداکثر می‌تواند ۵۰۰ کاراکتر باشد.");

     
        RuleFor(x => x.CollateralIssuer)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.CollateralIssuer))
            .WithMessage("صادرکننده وثیقه حداکثر می‌تواند ۲۰۰ کاراکتر باشد.");

        RuleFor(x => x.ChequeSerial)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.ChequeSerial))
            .WithMessage("سریال چک حداکثر می‌تواند ۲۰۰ کاراکتر باشد.");

    
        RuleFor(x => x.CollateralType)
            .IsInEnum()
            .WithMessage("نوع وثیقه (CollateralType) نامعتبر است.");
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

    private bool BeValidDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return false;

        return DateTime.TryParseExact(
            date,
            new[] { "yyyy/MM/dd" },
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }
}



