using Common;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using System.Text.Json;

namespace LoanGateway.Auth.Application.UseCase.Command.MobileOwner
{
    public sealed class VerifyMobileOwnerHandler(IShahkarRepository repository, IShahkarProvider provider) : IRequestHandler<VerifyMobileOwnerCommand, Result<VerifyMobileOwnerResultDto>>
    {
        private readonly IShahkarRepository _repository = repository;
        private readonly IShahkarProvider _provider = provider;
        public async Task<Result<VerifyMobileOwnerResultDto>> Handle(VerifyMobileOwnerCommand cmd, CancellationToken cancellationToken)
        {
            var requestedAt = DateTime.UtcNow;

            var response = await _provider.VerifyMobileOwnerAsync(cmd, cancellationToken);

            var responseReceivedAt = DateTime.UtcNow;


            var resultDto = new VerifyMobileOwnerResultDto
            {
                IsMatched = response.IsMatched,
                StatusCode = response.StatusCode,
                StatusMessage = response.StatusMessage,
                RequestId = response.RequestId,
                CorrelationId = response.CorrelationId
            };


            var log = new VerfiyMobileOwnerInquiry
            {
                NationalId = cmd.NationalId,
                MobileNumber = cmd.MobileNumber,
                IsMatched = response.IsMatched,
                StatusCode = response.StatusCode,
                StatusMessage = response.StatusMessage,
                RequestId = response.RequestId,
                CorrelationId = response.CorrelationId,
                RawResponseJson = JsonSerializer.Serialize(response)
            };

            await repository.InsertAsync(log, cancellationToken);

            return Result<VerifyMobileOwnerResultDto>.Success(resultDto);
        }
    }
}