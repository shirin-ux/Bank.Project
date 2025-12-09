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
public sealed class CompleteProfileCommandHandler(ICurrentUserService currentUser, IShahkarService shahkarService, IUserReadRepository userReadRepository) : IRequestHandler<CompleteProfileCommand,Result<ComplateProfileResultDto>>
{
    private readonly ICurrentUserService _currentUser= currentUser;
    private readonly IUserReadRepository _userReadRepository= userReadRepository;
    private readonly IShahkarService _shahkarService = shahkarService;



    public async Task<Result<ComplateProfileResultDto>> Handle(CompleteProfileCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        var user = await _userReadRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("کاربر یافت نشد.", AppErrorCodes.NotFound);

        if (!user.IsMobileVerified)
        {
            throw new LogicException(
                "ابتدا باید شماره موبایل شما تأیید شود.",
                AppErrorCodes.LogicError);
        }
        var mobile = _currentUser.MobileNumber ?? user.MobileNumber;
        var resShahkar = await _shahkarService.VerifyMobileOwnerAsync(request.NationalCode, mobile);
        if (!resShahkar.isMatched)
        {
            throw new LogicException(
                     "تطابق شماره موبایل و کدملی توسط سامانه شاهکار تأیید نشد.",
                     AppErrorCodes.ShahkarMismatch,
                     details: new
                     {
                         shahkarStatusCode = resShahkar.responseContext.status.code,
                         shahkarStatusMessage = resShahkar.responseContext.status.message
                     });
        }

        user.NationalCode = request.NationalCode;
        user.BirthDate = request.BirthDate;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsProfileCompleted = true;

        await _userReadRepository.UpdateProfileAsync(user, ct);
        var res = new ComplateProfileResultDto
        {

        };
        return Result<ComplateProfileResultDto>.Success(res);

    }
}
