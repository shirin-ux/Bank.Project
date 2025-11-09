
using LoanService.Domain.Entities;

namespace Bank.Mellat.Provider.Dtos;

public sealed class MellatPayResponseRes
{
    public PayRequestStatus? payRequestStatus { get; set; }
    public PayContractInfo? payContractInfo { get; set; }

    public sealed class PayRequestStatus
    {
        public PayResponseCode? responseCode { get; set; }       
        public string? responseMessage { get; set; } 
        public string? responseMessageCode { get; set; } 
    }

    public sealed class PayContractInfo
    {
        public string? nationalCode { get; set; }     
        public decimal? traceCode { get; set; }       
        public decimal? contractNo { get; set; }      
        public string? contractDate { get; set; }     
        public decimal? loanAmount { get; set; }      
        public decimal? sumCost { get; set; }         
        public short? installmentCount { get; set; }  
        public string? contractFile { get; set; }     
        public decimal? cbTrackingCode { get; set; }  
    }
}