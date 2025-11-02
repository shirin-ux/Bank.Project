using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    public sealed record GrantRequest(decimal ContractId, string? Status, string? PayRequestId, string? RequestedAmount, string? SignedContractBase64);

}
