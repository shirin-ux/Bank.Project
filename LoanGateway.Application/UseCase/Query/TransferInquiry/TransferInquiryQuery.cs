using Common;
using LoanService.Domain.Enum.Loan;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.TransferInquiry;

public sealed record TransferInquiryQuery
    : IRequest<TransferInquiryResultDto>
{
    public string RegisterCode { get; set; }
    public BankProviderType ProviderType { get; set; }
}