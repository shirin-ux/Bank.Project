
using Common;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.Enum;
using LoanGateway.Auth.Domain.Exceptions;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LoanGateway.Auth.Application.UseCase.Command.VerfiyOtp;

public sealed class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommandDto, Result<VerifyOtpResultDto>>
{
    private readonly IUserOtpRepository _repository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly OtpOptions _options;
    private readonly IJwtTokenService _jwtTokenService;
    public VerifyOtpCommandHandler(
        IUserOtpRepository repository,
         IUserReadRepository userReadRepository,
       IOptions<OtpOptions> options,
       IJwtTokenService jwtTokenService)
    {
        _repository = repository;
        _options = options.Value;
        _jwtTokenService = jwtTokenService;
        _userReadRepository = userReadRepository;
    }

    public async Task<Result<VerifyOtpResultDto>> Handle(VerifyOtpCommandDto request, CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;

        var otp = await _repository.GetActiveAsync(request.MobileNumber, request.Purpose, nowUtc, ct);

        if (otp is null)
        {
            throw new LogicException("کد تأیید نامعتبر است یا منقضی شده است.", AppErrorCodes.LogicError);
        }

        if (otp.FailedAttempts >= otp.MaxAttempts)
        {
            throw new LogicException(
                      "تعداد تلاش‌های ناموفق بیش از حد مجاز است. لطفاً کد جدید دریافت کنید.",
                      AppErrorCodes.LogicError,
                      details: new
                      {
                          blocked = true
                      });
        }

        var isValid = DateExtensions.Verify(
            request.Code,
            request.MobileNumber,
            request.Purpose,
            otp.CodeHash);

        if (!isValid)
        {
            var newFailed = otp.FailedAttempts + 1;
            await _repository.UpdateFailedAttemptsAsync(otp.Id, newFailed, ct);

            throw new LogicException("کد تأیید اشتباه است.", AppErrorCodes.LogicError,
                details: new
                {
                    remainingAttempts = Math.Max(0, otp.MaxAttempts - newFailed)
                });
        }

        await _repository.MarkConsumedAsync(otp.Id, DateTime.UtcNow, ct);

        User user;
        bool isNewUser = false;
        switch (request.Purpose)
        {
            case OtpPurpose.Login:
                user = await _userReadRepository.GetByMobileAsync(request.MobileNumber, ct)
                    ?? throw new NotFoundException(
                        "لطفا ثبت نام کنید.",
                        errorCode: AppErrorCodes.NotFound);

                if (!user.IsActive || !user.IsProfileCompleted)
                {
                    throw new LogicException(
                        !user.IsProfileCompleted
                            ? "برای ورود، ابتدا باید اطلاعات هویتی خود را تکمیل کنید."
                            : "حساب کاربری شما غیر فعال است.",
                        AppErrorCodes.LogicError);
                }
                break;

            case OtpPurpose.Register:

                var existing = await _userReadRepository.GetByMobileAsync(request.MobileNumber, ct);

                if (existing is not null)
                {
                    if (existing.IsProfileCompleted)
                    {
                        // کاربری که پروفایلش کامل است، دیگر نباید دوباره ثبت‌نام کند
                        throw new LogicException(
                            "این شماره موبایل قبلاً ثبت‌نام شده است.",
                            AppErrorCodes.LogicError);
                    }

                    // اگر قبلاً پروفایل کامل نشده و فقط ثبت‌نام نیمه‌کاره انجام شده،
                    // همان کاربر قبلی را برای صدور توکن برمی‌گردانیم تا بتواند دوباره تکمیل پروفایل را انجام دهد.
                    user = existing;
                    isNewUser = false;
                }
                else
                {
                    user = User.CreateNew(request.MobileNumber);

                    try
                    {
                        user.IsActive = true;
                        user = await _userReadRepository.InsertAsync(user, ct);
                        isNewUser = true;
                    }
                    catch (SqlException sqlEx) when (sqlEx.Number == 2627) 
                    {
                        throw new LogicException(
                            "این شماره موبایل قبلاً ثبت‌نام شده است.",
                            AppErrorCodes.LogicError);
                    }
                    catch (Exception ex) when (ex.InnerException is SqlException innerSqlEx && innerSqlEx.Number == 2627)
                    {
                        throw new LogicException(
                            "این شماره موبایل قبلاً ثبت‌نام شده است.",
                            AppErrorCodes.LogicError);
                    }
                }

                break;

            default:
                throw new LogicException("این نوع OTP هنوز برای این عملیات پشتیبانی نمی‌شود.", AppErrorCodes.LogicError);
        }
        var tokens = await _jwtTokenService.GenerateTokensAsync(user, ct);


        var result = new VerifyOtpResultDto
        {
            IsValid = true,
            Purpose = request.Purpose,
            UserId = user.Id,
            IsNewUser = isNewUser,
            AccessToken = tokens.AccessToken,
            AccessTokenExpiresAtUtc = tokens.AccessTokenExpiresAtUtc,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc,
            IsProfileCompleted = user.IsProfileCompleted
        };
        return Result<VerifyOtpResultDto>.Success(result);
    }
}