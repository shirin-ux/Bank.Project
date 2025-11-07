

using Common;

namespace LoanService.Application.UseCase.Command.RepaymentReques;

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
    public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContractBase64 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
