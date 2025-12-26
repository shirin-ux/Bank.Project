using Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Loan.Query.GetInstallments;

public sealed record GetInstallmentsResultDto:IBankResponse
{
    public decimal ContractNumber { get; init; }                
    public short InstallmentsCount { get; init; }               
    public short PaidInstallmentCount { get; init; }            
    public short NotPaidInstallmentCount { get; init; }         
    public string? EarlierInstallmentDate { get; init; }        
    public decimal InstallmentAmount { get; init; }             
    public decimal OpenDebtAmount { get; init; }                
    public decimal DebtAmount { get; init; }                    
    public decimal DiscountedDebtAmount { get; init; }          
    public string? ContractDesc { get; init; }                  
    public int? MessageCode { get; init; }
    public string? Message { get; init; }

    public List<InstallmentItemDto> Installments { get; init; } 
    public string[] NextActions { get; set; }
    public string State { get; set; }
    public string ContractBase64 { get; set; }
    public string RequestId { get; set; }
    public Dictionary<string, string[]>? Details { get; set; }

    public IEnumerable<BankStatusItem> GetStatusItems()
    {
        throw new NotImplementedException();
    }

    public sealed class InstallmentItemDto
    {
        public short InstallmentNo { get; init; }
        public short PaymentState { get; init; }      
        public int DueDate { get; init; }             
        public short DueState { get; init; }          
        public short Type { get; init; }              
        public decimal CapitalAmount { get; init; }
        public decimal InterestAmount { get; init; }
        public decimal PenaltyAmount { get; init; }
        public decimal InstallmentAmount { get; init; }
    }
}