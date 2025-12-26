using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.ValueObjects
{
    /// <summary>
    /// اطلاعات درخواستی تسهیلات مثل ContractId و غیره
    /// </summary>
    /// <param name="ContractId"></param>
    /// <param name="Status"></param>
    /// <param name="PayRequestId"></param>
    /// <param name="RequestedAmount"></param>
    /// <param name="SignedContractBase64"></param>
    public sealed record GrantRequest(
        decimal? ContractId,
        string? Status, 
        string? PayRequestId,
        string? RequestedAmount,
        string? SignedContractBase64);

}
