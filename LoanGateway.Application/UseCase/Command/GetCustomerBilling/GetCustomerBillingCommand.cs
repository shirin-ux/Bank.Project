using Common;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCustomerBilling;

public sealed record GetCustomerBillingCommand(
    short BillingNumber,   // 0 => آخرین 30 صورتحساب؛ غیرصفر => 30 صورتحساب کوچکتر از مقدار ورودی
    decimal ContractNumber,
    string NationalCode,
   BankProviderType ProviderType 
) : IRequest<GetCustomerBillingResultDto>;
