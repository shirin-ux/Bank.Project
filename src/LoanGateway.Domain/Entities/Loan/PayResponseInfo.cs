using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Entities.Loan
{
    /// <summary>
    /// پاسخ پرداخت از بانک
    /// </summary>
    /// <param name="Code"></param>
    /// <param name="BankContractNo"></param>
    /// <param name="ApprovedLoanAmount"></param>
    /// <param name="ContractDate"></param>
    /// <param name="CentralBankTraceCode"></param>
    /// <param name="BankSignedContractBase64"></param>
    /// <param name="ReceivedAtUtc"></param>
    public sealed class PayResponseInfo
    {
        public Guid Id { get; set; }
        public Guid LoanRequestId { get; set; }
        public PayResponseCode Code { get; set; }
       public decimal? BankContractNo { get; set; }
        public decimal? ApprovedLoanAmount { get; set; }
        public string? ContractDate { get; set; }
        public decimal? CentralBankTraceCode { get; set; }
        public string? BankSignedContractBase64 { get; set; }
        public DateTime? ReceivedAtUtc { get; set; }
    }
}