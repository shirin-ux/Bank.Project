using Common;
using LoanService.Domain.Enum.Loan;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.GetInstallments;

public sealed record GetInstallmentsQuery : IRequest<GetInstallmentsResultDto>
{
    public string NationalCode { get; set; }
    public string ContractNumber{get; set; }
    public BankProviderType ProviderType { get; set; }
}