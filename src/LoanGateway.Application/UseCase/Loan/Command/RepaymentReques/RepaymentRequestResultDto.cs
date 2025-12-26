

using Common;

namespace LoanService.Application.UseCase.Loan.Command.RepaymentReques;

public sealed record RepaymentRequestResultDto : IBankResponse
{
    public DateTime? RepaymentDate { get; init; }    
    public decimal? AccountNumber { get; init; }   
    public int? TrackNumber { get; init; }         
    public decimal? ContractNumber { get; init; }  
    public decimal? RepaymentAmount { get; init; } 
    public string? CustomerName { get; init; }

    public int? MessageCode { get; init; }

    public string? Message { get; init; }
    public string[] NextActions { get; set; }
    public string State { get; set; }
    public string ContractBase64 { get; set; }
    public string RequestId { get; set; }
    public Dictionary<string, string[]>? Details { get; set; }

    public IEnumerable<BankStatusItem> GetStatusItems()
    {
        throw new NotImplementedException();
    }
}
