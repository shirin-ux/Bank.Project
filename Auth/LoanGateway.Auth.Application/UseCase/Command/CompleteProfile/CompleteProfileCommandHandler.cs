using Common;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Domain.Exceptions;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using Shahkar.Provider;
using Shahkar.Provider.Dto;

namespace LoanGateway.Auth.Application.UseCase.Command.CompleteProfile;
public sealed class UserInfoHandler(IUserProfileRepository userProfileRepository,
    ICurrentUserService currentUser,
    IShahkarService shahkarService,
    IUserReadRepository userReadRepository, 
    IJwtTokenService jwtTokenService,
    IGiftCardEligibleUserRepository giftCardEligibleUserRepository) : IRequestHandler<CompleteProfileCommand, Result<ComplateProfileResultDto>>
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly IUserReadRepository _userReadRepository = userReadRepository;
    private readonly IShahkarService _shahkarService = shahkarService;
    private readonly IUserProfileRepository _userProfileRepository = userProfileRepository;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly IGiftCardEligibleUserRepository _giftCardEligibleUserRepository = giftCardEligibleUserRepository;



    public async Task<Result<ComplateProfileResultDto>> Handle(
        CompleteProfileCommand request,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId.ToString()))
        {
            throw new LogicException( "کاربر احراز هویت نشده است.", AppErrorCodes.Unauthorized);
        }

        var nowUtc = DateTime.UtcNow;

        var user = await _userProfileRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException(
                "کاربر یافت نشد.",
                AppErrorCodes.NotFound);

        //if (user.IsProfileCompleted)
        //{
        //    throw new LogicException(
        //        "اطلاعات هویتی شما قبلاً تکمیل شده است.",
        //        AppErrorCodes.LogicError);
        //}


        var exists = await _userProfileRepository.ExistsByNationalCodeAsync(
            request.NationalCode,
            user.MobileNumber,
            user.Id,
            ct);

        if (exists)
        {
            throw new LogicException("این کد ملی قبلاً برای یک کاربر دیگر ثبت شده است.",
                AppErrorCodes.LogicError);
        }

        ShahkarGetPersonInfoResponseDto personalInfo;
        try
        {
            var shahkar = await _shahkarService.VerifyMobileOwnerAsync(
                request.NationalCode,
                _currentUser.MobileNumber!,
                ct);

            if (!shahkar.isMatched)
            {
                throw new LogicException(
                    "تطابق شماره موبایل و کد ملی در سامانه شاهکار تأیید نشد.",
                    AppErrorCodes.ShahkarMismatch);
            }

            personalInfo = await _shahkarService.GetPersonalInfoAsync(
                request.NationalCode,
                request.BirthDate,
                ct);
        }
        catch (ExternalServiceException ex)
        {
            throw new LogicException(
                "استعلام اطلاعات هویتی در حال حاضر امکان‌پذیر نیست. لطفاً چند دقیقه دیگر دوباره تلاش کنید.",
                AppErrorCodes.ExternalServiceError,
                new
                {
                    externalSystem = ex.ExternalSystem,
                    externalStatusCode = ex.ExternalStatusCode
                });
        }
        // تا زمانی که تمام اعتبارسنجی‌ها و عملیات خارجی (مانند شاهکار و کارت هدیه) با موفقیت انجام نشود،
        // هیچ دیتایی از کاربر در دیتابیس آپدیت نمی‌شود. به این ترتیب اگر هر جا خطا بخورد، پروفایل کاربر تغییری نمی‌کند.

        var isEligible = await _giftCardEligibleUserRepository.IsEligibleByNationalCodeAsync(request.NationalCode, ct);
        if (isEligible)
        {
            // اگر به هر دلیل این متد خطا دهد، هنوز پروفایل کاربر آپدیت نشده است
            await _giftCardEligibleUserRepository.MarkAsProcessedAsync(request.NationalCode, ct);
        }

        
        user.CompleteProfileAfterKyc(
            nationalCode: request.NationalCode,
            firstName: personalInfo.basicInformation.firstName,
            lastName: personalInfo.basicInformation.lastName,
            birthdate: personalInfo.identificationInformation.birthDate,
            shenasnamehNumber:personalInfo.identificationInformation.shenasnamehNumber,
            shenasnameSerial:personalInfo.identificationInformation.shenasnameSerial,
            shenasnameSeri:personalInfo.identificationInformation.shenasnameSeri,
            nowUtc: nowUtc);

        if (isEligible)
        {
            user.GiftStatus = true;
        }

        await _userProfileRepository.UpdateProfileAfterKycAsync(user, ct);


        var result = new ComplateProfileResultDto
        {
            UserId = user.Id,
            FirstName = user.FirstName!,
            LastName = user.LastName!,
            NationalCode = user.NationalCode!,
            BirthDate = user.BirthDate!,
            IsProfileCompleted = user.IsProfileCompleted,
            ShenasnamehNumber=user.ShenasnamehNumber!,
            ShenasnameSeri=user.ShenasnameSeri!,
            ShenasnameSerial=user.ShenasnameSerial!
        };

        return Result<ComplateProfileResultDto>.Success(result);
    }
}