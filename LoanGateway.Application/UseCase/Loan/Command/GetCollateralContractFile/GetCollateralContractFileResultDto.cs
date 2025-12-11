

using Common;

namespace LoanService.Application.UseCase.Loan.Command.GetCollateralContractFile
{
    public sealed record GetCollateralContractFileResultDto : IBankResponse
    {
        public byte[] ContractFile { get; init; }
        public decimal ContractNumber { get; init; }
        public int? MessageCode { get; init; }
        public string? Message { get; init; }
        public string[] NextActions { get; set; }
        public string State { get; set; }
        public string ContractPath { get; set; }
        public string RequestId { get; set; }
        public Dictionary<string, string[]>? Details { get; set; }

        public IEnumerable<BankStatusItem> GetStatusItems()
        {
            throw new NotImplementedException();
        }
    }
}
