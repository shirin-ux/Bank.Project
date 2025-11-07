using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Entities
{
    /// <summary>
    /// اطلاعات انتقال وجه (حواله)
    /// </summary>
    /// <param name="RegisterCode"></param>
    /// <param name="TransactionNumber"></param>
    public sealed class TransferInfo
    {
        public Guid Id { get; set; }
        public Guid LoanRequestId { get; set; }
        public string? RegisterCode { get; set; }
        public decimal? TransactionNumber { get; set; }
    }
}
