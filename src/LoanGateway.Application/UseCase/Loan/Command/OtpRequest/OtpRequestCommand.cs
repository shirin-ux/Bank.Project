using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Loan.Command.OtpRequest;

public sealed record OtpRequestCommand : IRequest<OtpRequestResultDto>
{
    public decimal ContractNumber { get; set; }
    public string? AccountNumber { get; set; }
    public string? NationalCode { get; set; }
    public decimal PayAmount { get; set; }
    public ProviderType ProviderType { get; set; }

    public serviceType ServiceType { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

    public enum serviceType : short
    {
        deposit = 1,
        repayment = 2
    }
}
