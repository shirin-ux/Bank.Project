using Common;
using MediatR;

namespace LoanGateway.Auth.Application.UseCase.Command.MobileOwner
{
    public class VerifyMobileOwnerCommand : IRequest<Result<VerifyMobileOwnerResultDto>>
    {

        public string BirthDate { get; set; } = default!;
        public string NationalId { get; set; } = default!;
        public string MobileNumber { get; set; } = default!;
    }



}
