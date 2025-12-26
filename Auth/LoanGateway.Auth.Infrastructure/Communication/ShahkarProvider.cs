using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Application.UseCase.Command.MobileOwner;
using Shahkar.Provider;

namespace LoanGateway.Auth.Infrastructure.Communication
{
    public class ShahkarProvider(IShahkarService client) : IShahkarProvider
    {
        private readonly IShahkarService _client = client;

        public Task<VerifyMobileOwnerResultDto> GetPersonalInfoAsync(VerifyMobileOwnerCommand cmd, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<VerifyMobileOwnerResultDto> VerifyMobileOwnerAsync(VerifyMobileOwnerCommand cmd, CancellationToken cancellationToken = default)
        {

            var req = new
            {
                mobileNumber = cmd.MobileNumber,
                nationalId = cmd.NationalId

            };

            var resShahkar = await _client.VerifyMobileOwnerAsync(req.nationalId, req.mobileNumber, cancellationToken);
            return new VerifyMobileOwnerResultDto
            {
                CorrelationId = Guid.NewGuid().ToString(),
                IsMatched = resShahkar.isMatched,
                RequestId = resShahkar.responseContext.requestId,
                StatusCode = resShahkar.responseContext.status.code,
                StatusMessage = resShahkar.responseContext.status.message
            };
        }
    }
}
