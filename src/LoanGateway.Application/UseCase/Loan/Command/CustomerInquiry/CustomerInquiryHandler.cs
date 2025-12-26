using LoanService.Application.Contracts;
using MapsterMapper;
using MediatR;


namespace LoanService.Application.UseCase.Loan.Command.CustomerInquiry
{
    public sealed class CustomerInquiryHandler(
                                               IProviderFactory factory,
                                               IMapper mapper) : IRequestHandler<CustomerInquiryCommand, CustomerInquiryResultDto>
    {
        private readonly IProviderFactory _factory = factory;

        public async Task<CustomerInquiryResultDto> Handle(CustomerInquiryCommand command, CancellationToken ct)
        {
            var provider = _factory.GetProvider<IProvider>(command.ProviderType);
            return await provider.CustomerInquiryAsync(command, ct);
        }
    }
}
