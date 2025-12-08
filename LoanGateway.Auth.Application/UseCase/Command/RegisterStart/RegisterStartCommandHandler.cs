using Common;
using LoanGateway.Auth.Application.Common;
using LoanGateway.Auth.Domain.Enum;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LoanGateway.Auth.Application.UseCase.Command.RegisterStart;

public sealed class RegisterStartCommandHandler
    : IRequestHandler<RegisterStartCommand, Result<RegisterStartResultDto>>
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly IUserOtpRepository _otpRepository;
    private readonly ISmsSender _smsSender;
    private readonly IDateTimeProvider _dateTime;

    public RegisterStartCommandHandler(
        IUserReadRepository userReadRepository,
        IUserOtpRepository otpRepository,
        ISmsSender smsSender,
        IDateTimeProvider dateTime)
    {
        _userReadRepository = userReadRepository;
        _otpRepository = otpRepository;
        _smsSender = smsSender;
        _dateTime = dateTime;
    }

    public async Task<Result<RegisterStartResultDto>> Handle(RegisterStartCommand request, CancellationToken ct)
    {

        var exists = await _userReadRepository
            .ExistsByMobileOrNationalCodeAsync(request.MobileNumber, request.NationalCode, ct);

        if (exists)
        {
            return Result<RegisterStartResultDto>.Failure(new Error(-1, "کاربری با این شماره موبایل یا کدملی قبلاً ثبت شده است."));
        }


        var now = _dateTime.UtcNow;
        var activeOtpCount = await _otpRepository.CountRequestsInWindowAsync(request.MobileNumber, OtpPurpose.Register, now, ct);

        if (activeOtpCount >= 3)
        {
            return Result<RegisterStartResultDto>.Failure(new Error(-2, "تعداد درخواست کد تأیید بیش از حد مجاز است. لطفاً بعداً تلاش کنید."));
        }


        var otpCode = DateExtensions.GenerateOtpCode(6);

        var expiresAtUtc = now.AddMinutes(2);


        await _otpRepository.InsertAsync(new Domain.Entities.OtpCode { }, ct);

        var message = $"کد تأیید ثبت‌نام شما: {otpCode}";
        await _smsSender.SendAsync(request.MobileNumber, message, ct);
        var res = new RegisterStartResultDto
        {
            IsRegister = true
        };
        return Result<RegisterStartResultDto>.Success(res);
    }


}
