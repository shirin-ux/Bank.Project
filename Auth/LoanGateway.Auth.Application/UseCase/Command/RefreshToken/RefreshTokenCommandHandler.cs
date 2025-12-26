using Common;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoanGateway.Auth.Application.UseCase.Command.RefreshToken;



public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommandDto, Result<RefreshTokenResultDto>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserReadRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;


    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserReadRepository userRepository,
        IJwtTokenService jwtTokenService,
      ILogger<RefreshTokenCommandHandler> logger
   )
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<RefreshTokenResultDto>> Handle(
     RefreshTokenCommandDto request, CancellationToken ct)
    {
        //    var nowUtc = DateTime.UtcNow;

        //    // 1. پیدا کردن توکن قبلی بر اساس refresh token
        //    var oldToken = await _refreshTokenRepository.GetByPlainRefreshTokenAsync(request.RefreshToken, ct);
        //    if (oldToken == null)
        //        return Result<RefreshTokenResultDto>.Failure(new Error(-1, "توکن رفرش نامعتبر یا منقضی شده است."));

        //    // 2. بررسی اینکه توکن قبلا استفاده نشده باشد
        //    if (oldToken.RevokedAtUtc != null)
        //        return Result<RefreshTokenResultDto>.Failure(new Error(-1, "این توکن قبلا استفاده شده است."));

        //    // 3. بررسی مطابقت با AccessToken
        //    if (oldToken.AccessToken != request.AccessToken)
        //        return Result<RefreshTokenResultDto>.Failure(new Error(-1, "این رفرش توکن برای این اکسس توکن نیست."));

        //    // 4. گرفتن کاربر
        //    var user = await _userRepository.GetByIdAsync(oldToken.UserId, ct);
        //    if (user == null)
        //        return Result<RefreshTokenResultDto>.Failure(new Error(404, "کاربر مربوط به این توکن یافت نشد."));

        //    // 5. revoke کردن توکن قبلی
        //    oldToken.RevokedAtUtc = nowUtc;
        //    oldToken.RevokedReason = "Rotated";
        //    await _refreshTokenRepository.UpdateAsync(oldToken, ct);

        //    // 6. ایجاد توکن جدید
        //    var pair = await _jwtTokenService.GenerateTokensAsync(user, ct);

        //    var newToken = new Domain.Entities.RefreshToken
        //    {
        //        Id = Guid.NewGuid(),
        //        UserId = user.Id,
        //        AccessToken = pair.AccessToken,
        //        PlainRefreshToken = pair.RefreshToken,
        //        AccessTokenExpiresAtUtc = pair.AccessTokenExpiresAtUtc,
        //        ExpiresAtUtc = pair.RefreshTokenExpiresAtUtc,

        //    };

        //    await _refreshTokenRepository.InsertAsync(newToken, ct);

        //    return Result<RefreshTokenResultDto>.Success(new RefreshTokenResultDto
        //    {
        //        AccessToken = pair.AccessToken,
        //        RefreshToken = pair.RefreshToken,
        //        AccessTokenExpiresAtUtc = pair.AccessTokenExpiresAtUtc,
        //        RefreshTokenExpiresAtUtc = pair.RefreshTokenExpiresAtUtc
        //    });
        //}
        var nowUtc = DateTime.UtcNow;

        // لاگ کردن refresh token دریافتی (بدون نمایش کامل برای امنیت)
        var tokenPrefix = request.RefreshToken != null && request.RefreshToken.Length > 0
            ? request.RefreshToken.Substring(0, Math.Min(10, request.RefreshToken.Length))
            : "null";
        
        _logger.LogInformation(
            "Refresh token received. TokenLength: {TokenLength}, TokenPrefix: {TokenPrefix}",
            request.RefreshToken?.Length ?? 0,
            tokenPrefix);

        var oldToken = await _refreshTokenRepository.GetByPlainRefreshTokenAsync(request.RefreshToken, ct);
        if (oldToken == null)
        {
            _logger.LogWarning(
                "Refresh token not found in database. TokenLength: {TokenLength}, TokenPrefix: {TokenPrefix}",
                request.RefreshToken?.Length ?? 0,
                tokenPrefix);
            return Result<RefreshTokenResultDto>.Failure(new Error(-1, "توکن نامعتبر/منقضی شده است."));
        }

        _logger.LogInformation(
            "Refresh token found for user {UserId}. ExpiresAt: {ExpiresAt}, Now: {Now}, IsExpired: {IsExpired}",
            oldToken.UserId, 
            oldToken.ExpiresAtUtc, 
            nowUtc, 
            oldToken.ExpiresAtUtc <= nowUtc);

        if (oldToken.ExpiresAtUtc <= nowUtc)
        {
            _logger.LogWarning("Refresh token expired for user {UserId}. ExpiredAt: {ExpiredAt}, Now: {Now}",
                oldToken.UserId, oldToken.ExpiresAtUtc, nowUtc);
            return Result<RefreshTokenResultDto>.Failure(
                new Error(-1, "توکن منقضی شده است."));
        }

        if (oldToken.RevokedAtUtc != null)
        {
            _logger.LogWarning("Refresh token already revoked for user {UserId}. RevokedAt: {RevokedAt}, Reason: {Reason}. Possible token reuse attack!",
                oldToken.UserId, oldToken.RevokedAtUtc, oldToken.RevokedReason);

            return Result<RefreshTokenResultDto>.Failure(
                new Error(-1, " توکن قبلا استفاده شده است."));
        }

        var user = await _userRepository.GetByIdAsync(oldToken.UserId, ct);
        if (user == null)
        {
            _logger.LogWarning("User not found for refresh token. UserId: {UserId}", oldToken.UserId);
            return Result<RefreshTokenResultDto>.Failure(
                new Error(404, "کاربر یافت نشد."));
        }

        oldToken.RevokedAtUtc = nowUtc;
        oldToken.RevokedReason = "Rotated";
        oldToken.RotatedAtUtc = nowUtc;
        await _refreshTokenRepository.UpdateAsync(oldToken, ct);

        _logger.LogInformation("Old refresh token revoked for user {UserId}. Rotating to new token.", user.Id);

        var pair = await _jwtTokenService.GenerateTokensAsync(user, ct);

        var newToken = new Domain.Entities.RefreshTokens
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = pair.AccessToken,
            RefreshToken = pair.RefreshToken,
            AccessTokenExpiresAtUtc = pair.AccessTokenExpiresAtUtc,
            ExpiresAtUtc = pair.RefreshTokenExpiresAtUtc,
            ReplacedByTokenId = null,
        };


        newToken.ReplacedByTokenId = oldToken.Id;

        await _refreshTokenRepository.InsertAsync(newToken, ct);

        _logger.LogInformation("New refresh token created for user {UserId}", user.Id);

        return Result<RefreshTokenResultDto>.Success(new RefreshTokenResultDto
        {
            AccessToken = pair.AccessToken,
            RefreshToken = pair.RefreshToken,
            AccessTokenExpiresAtUtc = pair.AccessTokenExpiresAtUtc,
            RefreshTokenExpiresAtUtc = pair.RefreshTokenExpiresAtUtc
        });
    }
}


