using LoanGateway.Auth.Application.UseCase.Command.MobileOwner;

namespace LoanGateway.Auth.Application.Common
{
    public interface IShahkarProvider
    {
        Task<VerifyMobileOwnerResultDto> VerifyMobileOwnerAsync(VerifyMobileOwnerCommand cmd, CancellationToken cancellationToken = default);
        Task<VerifyMobileOwnerResultDto> GetPersonalInfoAsync(VerifyMobileOwnerCommand cmd, CancellationToken cancellationToken = default);
    }
}
