using Common;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.RepaymentReques;
public sealed record RepaymentRequestCommand : IRequest<RepaymentRequestResultDto>
{
    public string? AccountNo { get; init; }          
    public decimal? ContractNo { get; init; }         
    public string? NationalCode { get; init; } = default!; 
    public decimal? RepaymentAmount { get; init; }
    public BankProviderType ProviderType { get; set; }
    public string? OtpCode { get; init; }            
}