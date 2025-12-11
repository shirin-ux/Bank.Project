using Common;
using LoanGateway.Auth.Application.Common;
using LoanGateway.Auth.Application.UseCase.Command.RequestOtp;
using LoanGateway.Auth.Domain.Exceptions;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using Shahkar.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.CompleteProfile;
public sealed class CompleteProfileCommandHandler(IUserProfileRepository userProfileRepository,
    ICurrentUserService currentUser,
    IShahkarService shahkarService,
    IUserReadRepository userReadRepository, IJwtTokenService jwtTokenService) : IRequestHandler<CompleteProfileCommand,Result<ComplateProfileResultDto>>
{
    private readonly ICurrentUserService _currentUser= currentUser;
    private readonly IUserReadRepository _userReadRepository= userReadRepository;
    private readonly IShahkarService _shahkarService = shahkarService;
    private readonly IUserProfileRepository _userProfileRepository= userProfileRepository;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;



    public async Task<Result<ComplateProfileResultDto>> Handle(
        CompleteProfileCommand request,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId.ToString()))
        {
            throw new LogicException(
                "کاربر احراز هویت نشده است.",
                AppErrorCodes.Unauthorized);
        }

        var nowUtc = DateTime.UtcNow;

        var user = await _userProfileRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException(
                "کاربر یافت نشد.",
                AppErrorCodes.NotFound);

        if (!user.IsActive)
        {
            throw new LogicException(
                "حساب کاربری شما غیر فعال است.",
                AppErrorCodes.LogicError);
        }

        if (user.IsProfileCompleted)
        {
            throw new LogicException(
                "اطلاعات هویتی شما قبلاً تکمیل شده است.",
                AppErrorCodes.LogicError);
        }


        var exists = await _userProfileRepository.ExistsByNationalCodeAsync(
            request.NationalCode,
            user.Id,
            ct);

        if (exists)
        {
            throw new LogicException(
                "این کد ملی قبلاً برای یک کاربر دیگر ثبت شده است.",
                AppErrorCodes.LogicError);
        }

        var shahkar = await _shahkarService.VerifyMobileOwnerAsync( request.NationalCode, _currentUser.MobileNumber!,ct);

        if (!shahkar.isMatched)
        {
            throw new LogicException( "تطابق شماره موبایل و کد ملی در سامانه شاهکار تأیید نشد.", AppErrorCodes.LogicError);
        }

        var personalInfo = await _shahkarService.GetPersonalInfoAsync( request.NationalCode, request.BirthDate, ct);


        //var birthDateFromUser = request.BirthDateJalali.ToGregorianDate();
        //if (birthDateFromUser.Date != personalInfo.BirthDate.Date)
        //{
        //    throw new LogicException(
        //        "تاریخ تولد وارد شده با اطلاعات ثبت احوال مغایرت دارد.",
        //        AppErrorCodes.LogicError);
        //}


        user.CompleteProfileAfterKyc(
            nationalCode: request.NationalCode,
            firstName: personalInfo.basicInformation.firstName,
            lastName: personalInfo.basicInformation.lastName,
            birthdate:personalInfo.identificationInformation.birthDate,
            nowUtc: nowUtc);

        await _userProfileRepository.UpdateProfileAfterKycAsync(user, ct);


        var tokens = await _jwtTokenService.GenerateTokensAsync(user, ct);

        var result = new ComplateProfileResultDto
        {
            UserId = user.Id,
            FirstName = user.FirstName!,
            LastName = user.LastName!,
            NationalCode = user.NationalCode!,
            BirthDate = user.BirthDate,
            IsProfileCompleted = user.IsProfileCompleted,
            AccessToken = tokens.AccessToken,
            AccessTokenExpiresAtUtc = tokens.AccessTokenExpiresAtUtc,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc
        };

        return Result<ComplateProfileResultDto>.Success(result);
    }
}