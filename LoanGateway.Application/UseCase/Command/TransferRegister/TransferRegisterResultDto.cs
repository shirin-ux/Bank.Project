

using Common;

namespace LoanService.Application.UseCase.Command.TransferRegister;
public sealed record TransferRegisterResultDto:IBankResponse
{
    public string? RegisterCode { get; init; }       
    public short? TransType { get; init; }
    public  List<string> ContractsError { get; init; } = new();
    public List<string> TransactionsError { get; init; } = new();

    public int? MessageCode { get; init; }

    public string? Message { get; init; }
    public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContractBase64 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IEnumerable<BankStatusItem> GetStatusItems()
    {
        throw new NotImplementedException();
    }
}
