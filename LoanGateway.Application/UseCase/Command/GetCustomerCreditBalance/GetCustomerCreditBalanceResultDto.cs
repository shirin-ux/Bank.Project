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
    public List<ContractCreditList> contractCreditList { get; init; } 
    public int? MessageCode { get; init; }
    public string? Message { get; init; }
    public string[] NextActions { get; init; }
    public string State { get; set; }

    public string RequestId { get; set; }
    public Dictionary<string, string[]>? Details { get; set; }

    public IEnumerable<BankStatusItem> GetStatusItems()
    {
        throw new NotImplementedException();
    }

    public sealed record ContractCreditList
    {
        public decimal ApprovalCode { get; init; }
        public decimal ContractNumber { get; init; }
        public decimal CreditBalance { get; init; }
    }
}
