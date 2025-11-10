using Common;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.OtpRequest;

public sealed record OtpRequestCommand : IRequest<OtpRequestResultDto>
{
    public decimal ContractNumber { get; set; }      
    public string AccountNumber { get; set; } 
    public string NationalCode { get; set; } 
    public decimal PayAmount { get; set; } 
    public BankProviderType ProviderType { get; set; }

    public serviceType ServiceType { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

    public enum serviceType:short
    {
    deposit=1,
    repayment=2
    }
}
