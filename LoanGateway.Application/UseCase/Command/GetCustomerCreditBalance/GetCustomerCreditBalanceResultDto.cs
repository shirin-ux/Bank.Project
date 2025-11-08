using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCustomerCreditBalance;

public sealed record GetCustomerCreditBalanceResultDto:IBankResponse
{
    public string NationalCode { get; init; } = default!;
    public IReadOnlyList<ContractCreditItemDto> ContractCreditList { get; init; } = Array.Empty<ContractCreditItemDto>();
    public int? MessageCode { get; init; }
    public string? Message { get; init; }
    public string[] NextActions { get; init; }
    public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContractBase64 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IEnumerable<BankStatusItem> GetStatusItems()
    {
        throw new NotImplementedException();
    }

    public sealed record ContractCreditItemDto
    {
        public decimal ApprovalCode { get; init; }
        public decimal ContractNumber { get; init; }
        public decimal CreditBalance { get; init; }
    }
}
