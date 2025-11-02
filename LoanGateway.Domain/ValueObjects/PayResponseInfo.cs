using LoanService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    public sealed record PayResponseInfo(
        PayResponseCode Code,
        decimal BankContractNo,
        decimal? ApprovedLoanAmount,
        string? ContractDate,
        decimal? CentralBankTraceCode,
        string? BankSignedContractBase64,
        DateTime? ReceivedAtUtc
    );
}
