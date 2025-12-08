using Common;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.VerfiyOtp;

public sealed class VerifyOtpCommandHandler: IRequestHandler<VerifyOtpCommandDto, Result<VerifyOtpResultDto>>
{
    private readonly IUserOtpRepository _repository;
    private readonly OtpOptions _options;

    public VerifyOtpCommandHandler(
        IUserOtpRepository repository,
       IOptions<OtpOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    public async Task<Result<VerifyOtpResultDto>> Handle(VerifyOtpCommandDto request, CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;

        var otp = await _repository.GetActiveAsync(request.PhoneNumber,request.Purpose,nowUtc,ct);

        //if (otp is null || otp.IsExpired(nowUtc) || otp.IsDeleted)
        //{
        //    return Result<VerifyOtpResultDto>.Failure(new Error(-1, "کد نامعتبر است یا منقضی شده است."));
        //}

        if (otp.FailedAttempts >= otp.MaxAttempts)
        {
            return Result<VerifyOtpResultDto>.Failure(new Error(-1, "تعداد تلاش‌های ناموفق بیش از حد مجاز است. لطفاً کد جدید دریافت کنید."));
        }

        var isValid = DateExtensions.Verify(
            request.Code,
            request.PhoneNumber,
            request.Purpose,
            otp.CodeHash);

        if (!isValid)
        {
            otp.RegisterFailedAttempt();
            await _repository.UpdateAsync(otp, ct);

            return Result<VerifyOtpResultDto>.Failure(new Error(-1,"کد نامعتبر است."));
        }


        otp.Consume(nowUtc);
        await _repository.UpdateAsync(otp, ct);

        // اینجا می‌تونی براساس Purpose ادامه جریان رو انجام بدی
        // مثال:
        // if (request.Purpose == OtpPurpose.Login)
        //    => تولید JWT, RefreshToken, و برگردوندن به User
        // فعلاً فقط Success ساده برمی‌گردونیم.
        var res = new VerifyOtpResultDto
        {

        };
        return Result<VerifyOtpResultDto>.Success(res);
    }
}