using Common;
using MediatR;

namespace LoanGateway.Auth.Application.UseCase.Command.CompleteProfile
{
    public sealed class CompleteProfileCommand : IRequest<Result<ComplateProfileResultDto>>
    {
        public string NationalCode { get; init; } = default!;
        public DateTime BirthDate { get; init; }
        public string FirstName { get; init; } = default!;
        public string LastName { get; init; } = default!;
    }
}
