using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Enum
{
    public enum LoanStatus
    {
        Created = 0,
        InquiryRegistered = 1,
        InquiryRejected = 2,
        InquiryApproved = 3,
        ContractReady = 4,
        ContractAccepted = 5,
        PaySubmitted = 6,
        Granted = 7,
        Failed = 8
    }
}
