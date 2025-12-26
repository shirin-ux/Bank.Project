using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{

    public sealed record ContractInfo(
        string? ApprovalCode,// به‌صورت string ذخیره کن (ملت: DECIMAL؛ سامان ممکن است فرق کند)
         bool  WithCollateral,
        decimal ContractNumber,
        string? Desc,
        string? SignedContractBase64
    );
}
