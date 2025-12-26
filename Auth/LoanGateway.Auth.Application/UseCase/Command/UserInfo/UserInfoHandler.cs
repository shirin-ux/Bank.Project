using Common;
using LoanGateway.Auth.Domain.Exceptions;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;

namespace LoanGateway.Auth.Application.UseCase.Command.UserInfo;
public sealed class UserInfoHandler(IUserProfileRepository userProfileRepository) : IRequestHandler<UserInfoCommand, Result<UserInfoResultDto>>
{

    private readonly IUserProfileRepository _userProfileRepository = userProfileRepository;

    public async Task<Result<UserInfoResultDto>> Handle(UserInfoCommand request, CancellationToken cancellationToken)
    {
        var user = await _userProfileRepository.GetByNationalCodeAsync(request.NationalCode, cancellationToken)
            ?? throw new NotFoundException(
                "کاربر یافت نشد.",
                AppErrorCodes.NotFound);

        if (!user.IsActive)
        {
            throw new LogicException(
                "حساب کاربری شما غیر فعال است.",
                AppErrorCodes.LogicError);
        }

        //if (user.IsProfileCompleted)
        //{
        //    throw new LogicException(
        //        "اطلاعات هویتی شما قبلاً تکمیل شده است.",
        //        AppErrorCodes.LogicError);
        //}



        var result = new UserInfoResultDto
        {
            PostalCode = user.PostalCode,
            NationalCode = user.NationalCode!,
            BirthDate = user.BirthDate,
            MobileNumber = user.MobileNumber,
            UserId=user.Id
        };

        return Result<UserInfoResultDto>.Success(result);
    }
}