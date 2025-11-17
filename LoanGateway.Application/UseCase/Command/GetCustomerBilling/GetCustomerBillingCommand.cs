using Common;
using LoanService.Domain.Enum.Loan;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCustomerBilling;

public sealed record GetCustomerBillingCommand(
    short BillingNumber,  
    string ContractNumber,
    string NationalCode,
   BankProviderType ProviderType 
) : IRequest<GetCustomerBillingResultDto>;
