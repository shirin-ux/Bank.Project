using Common;
using MediatR;

namespace LoanGateway.Auth.Application.UseCase.Command.CompleteProfile;

public sealed class CompleteProfileCommand : IRequest<Result<ComplateProfileResultDto>>
{
    public string NationalCode { get; init; } = default!;
    public string BirthDate { get; init; }
}
