using Common;
using LoanService.Domain.Entities;

namespace LoanService.Application.UseCase.Query.PayResponse;

public sealed record GetPayResponseResultDto:IBankResponse
{
    //public PayRequestStatusDto PayRequestStatus { get; set; } = new();
    public PayContractInfoDto PayContractInfo { get; set; }

    public string Message { get; init; }
    public int? MessageCode { get; init; }
    public string[] NextActions { get; set; }
    public string State { get; set; }
  
    public string RequestId { get; set; }

    public IEnumerable<BankStatusItem> GetStatusItems()
    {
        throw new NotImplementedException();
    }

    //public sealed record PayRequestStatusDto
    //{

    //    public int? responseCode { get; set; }
    //    public string? responseMessage { get; set; }
    //    public string? responseMessageCode { get; set; }

    //}

    public sealed record PayContractInfoDto
    {
        public string? NationalCode { get; init; }
        public decimal? TraceCode { get; init; }
        public decimal? ContractNo { get; init; }
        public string? ContractDate { get; init; }
        public decimal? LoanAmount { get; init; }
        public decimal? SumCost { get; init; }
        public short? InstallmentCount { get; init; }
        public string? ContractFile { get; init; }
        public decimal? CbTrackingCode { get; init; }
    }
    public enum PayResponseCode : int
    {
        Unknown = 0,
        Pending = 1,
        Success = 2,
        Failed = 3,
        NotAllowed = 4
    }
}
