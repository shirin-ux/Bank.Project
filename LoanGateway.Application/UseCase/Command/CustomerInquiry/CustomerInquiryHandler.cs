using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Enum;
using MapsterMapper;
using MediatR;


namespace LoanService.Application.UseCase.Command.CustomerInquiry
{
    public sealed class CustomerInquiryHandler(
         IBankProviderFactory factory,
         IMapper mapper)
         : IRequestHandler<CustomerInquiryCommand,CustomerInquiryResultDto>
    {



        private readonly IBankProviderFactory _factory = factory;

        public async Task<CustomerInquiryResultDto> Handle(CustomerInquiryCommand command, CancellationToken ct)
        {
            var provider = _factory.GetProvider(command.ProviderType);
            return await provider.CustomerInquiryAsync(command, ct);
        }
    }
}
