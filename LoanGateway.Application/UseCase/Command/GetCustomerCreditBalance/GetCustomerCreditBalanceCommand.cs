using Common;
using LoanService.Domain.Enum.Loan;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCustomerCreditBalance;

public sealed record GetCustomerCreditBalanceCommand : IRequest<GetCustomerCreditBalanceResultDto>
{
    public BankProviderType ProviderType { get; set; }
    public string ContractNumber { get; set; }
   public string NationalCode { get; set; }
}