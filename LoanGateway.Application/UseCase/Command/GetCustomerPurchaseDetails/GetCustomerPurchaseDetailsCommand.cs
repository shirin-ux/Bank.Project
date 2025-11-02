using Common;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCustomerPurchaseDetails;

public sealed record GetCustomerPurchaseDetailsCommand(
    decimal ContractNumber,
    BankProviderType ProviderType ,
    string NationalCode,
    string FromDate, // DD/MM/YYYY
    string ToDate    // DD/MM/YYYY
) : IRequest<GetCustomerPurchaseDetailsResultDto>;
