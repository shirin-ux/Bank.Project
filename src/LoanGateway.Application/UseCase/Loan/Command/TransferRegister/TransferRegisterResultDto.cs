

using Common;

namespace LoanService.Application.UseCase.Loan.Command.TransferRegister;
public sealed record TransferRegisterResultDto : IBankResponse
{
    public string? RegisterCode { get; init; }       
    public short? TransType { get; init; }
    public  List<string> ContractsError { get; init; } = new();
    public List<string> TransactionsError { get; init; } = new();

 
    public string[] NextActions { get; init; }
    public string State { get; init; }

    public int? MessageCode { get; set; }

    public string? Message { get; set; }

    public Dictionary<string, string[]>? Details { get; set; }
    public string RequestId { get; set; }

    public IEnumerable<BankStatusItem> GetStatusItems()
    {
        throw new NotImplementedException();
    }
}
