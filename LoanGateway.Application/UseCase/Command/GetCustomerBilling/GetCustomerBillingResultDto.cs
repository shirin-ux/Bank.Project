using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCustomerBilling;

public sealed record GetCustomerBillingResultDto : IBankResponse
{
    public IReadOnlyList<BillingItemDto> Billings { get; init; } = Array.Empty<BillingItemDto>();
    public string? Message { get; init; }
    public int? MessageCode { get; init; }
    public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContractBase64 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public sealed record BillingItemDto
    {
        public string? CustomerName { get; init; }                
        public int? PayDeadline { get; init; }                  
        public decimal? TotalPurchase { get; init; }              
        public decimal? ContractNumber { get; init; }             
        public decimal? DebtPayableInInstallments { get; init; }  
        public decimal? PayableAmount { get; init; }              
        public int? IssueDate { get; init; }                      
        public int? PeriodStartDate { get; init; }                
        public int? PeriodEndDate { get; init; }                  
        public short? BillingNumber { get; init; }                
    }
}