using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCustomerPurchaseDetails;


public sealed record GetCustomerPurchaseDetailsResultDto:IBankResponse
{
    public decimal ContractNumber { get; init; }
    public decimal NationalCode { get; init; }
    public string CustomerName { get; init; } = default!;
    public int LoanTypeCode { get; init; }
    public string LoanTypeDesc { get; init; } = default!;
    public decimal ContractAmount { get; init; }
    public decimal LoanPaiedAmount { get; init; }
    public IReadOnlyList<ContractDetailDto> ContractDetails { get; init; } = Array.Empty<ContractDetailDto>();
    public string? Message { get; init; }
    public int? MessageCode { get; init; }
    public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContractBase64 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public sealed record ContractDetailDto
    {
        public string TransactionDate { get; init; } = default!;
        public decimal TransactionNumber { get; init; }
        public decimal PayAccNumber { get; init; }
        public decimal UsedCreditAmount { get; init; }
        public decimal SellerPaiedAmount { get; init; }
        public string DocDate { get; init; } = default!;
        public string SellerNationalCode { get; init; } = default!;
        public string SellerName { get; init; } = default!;
    }
}
