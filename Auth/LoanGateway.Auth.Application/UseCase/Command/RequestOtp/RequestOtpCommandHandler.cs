using Common;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace LoanGateway.Auth.Application.UseCase.Command.RequestOtp;

public sealed class RequestOtpCommandHandler : IRequestHandler<RequestOtpCommandDto, Result<RequestOtpResultDto>>
{
    private readonly IUserOtpRepository _repository;
    private readonly ISmsSender _smsSender;
    private readonly IOptions<OtpOptions> _options;
    private readonly ILogger<RequestOtpCommandHandler> _logger;
    public RequestOtpCommandHandler(
        IUserOtpRepository repository,
        IOptions<OtpOptions> options,
        ISmsSender smsSender,
        ILogger<RequestOtpCommandHandler> logger)
    {
        _repository = repository;
        _smsSender = smsSender;
        _options = options;
        _logger = logger;

    }

    public async Task<Result<RequestOtpResultDto>> Handle(RequestOtpCommandDto request, CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        var windowStart = nowUtc.AddMinutes(-_options.Value.RequestWindowMinutes);
        var count = await _repository.CountRequestsInWindowAsync(
            request.PhoneNumber,
            request.Purpose,
            windowStart,
            ct);

        if (count >= _options.Value.MaxRequestsPerWindow)
        {
            return Result<RequestOtpResultDto>.Failure(
                new Error(-1, "تعداد درخواست‌های مجاز برای این شماره در بازه زمانی کوتاه، بیش از حد است."));
        }

        // 2. بررسی rate limit بر اساس IP (اگر IP موجود باشد)
        //if (!string.IsNullOrWhiteSpace(request.RequestIp))
        //{
        //    var ipWindowStart = nowUtc.AddMinutes(-_options.Value.RequestWindowMinutes);
        //    var ipCount = await _repository.CountRequestsByIpInWindowAsync(
        //        request.RequestIp,
        //        ipWindowStart,
        //        ct);

        //    // محدودیت IP معمولاً 3 برابر محدودیت شماره تلفن است (برای جلوگیری از abuse)
        //    var maxIpRequests = _options.Value.MaxRequestsPerWindow * 3;

        //    if (ipCount >= maxIpRequests)
        //    {
        //        return Result<RequestOtpResultDto>.Failure(
        //            new Error(-1, "تعداد درخواست‌های مجاز برای این IP در بازه زمانی کوتاه، بیش از حد است."));
        //    }
        //}



        var existing = await _repository.GetActiveAsync(request.PhoneNumber, request.Purpose, nowUtc, ct);

        if (existing is not null)
        {
            existing.IsDeleted = true;
            existing.ConsumedAtUtc = nowUtc;
            await _repository.UpdateAsync(existing, ct);
        }

        var code = DateExtensions.GenerateOtpCode(_options.Value.CodeLength);

        var hash = DateExtensions.Hash(code, request.PhoneNumber, request.Purpose);

        var otp = new OtpCode
        {
            Id= Guid.NewGuid(),
            UserId = null, 
            PhoneNumber = request.PhoneNumber,
            Purpose = request.Purpose,
            CodeHash = hash,
            ExpiresAtUtc = nowUtc.AddMinutes(_options.Value.ExpiryMinutes),
            ConsumedAtUtc = null,
            FailedAttempts = 0,
            MaxAttempts = _options.Value.MaxAttempts,
            //RequestIp = request.RequestIp,
            //UserAgent = request.UserAgent,
            IsDeleted = false
        };

        await _repository.InsertAsync(otp, ct);


        var message = $"کد ورود شما: {code} \nاین کد تا {_options.Value.ExpiryMinutes} دقیقه معتبر است.";
        await _smsSender.SendAsync(request.PhoneNumber, message, ct);
        var res = new RequestOtpResultDto
        { 

        };
        return Result<RequestOtpResultDto>.Success(res);
    }

}