using Common;
using LoanService.Domain.Entities;

namespace LoanService.Application.UseCase.Query.PayResponse;

public sealed record GetPayResponseResultDto:IBankResponse
{
    public PayRequestStatusDto PayRequestStatus { get; init; } = new();
    public PayContractInfoDto PayContractInfo { get; init; }

    public string Message { get; init; }
    public int? MessageCode { get; init; }
    public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContractBase64 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public sealed record PayRequestStatusDto
    {
        public PayResponseCode ResponseCode { get; init; }
   
    }

    public sealed record PayContractInfoDto
    {
        public string? NationalCode { get; init; }
        public decimal? TraceCode { get; init; }
        public decimal ContractNo { get; init; }
        public string? ContractDate { get; init; }
        public decimal? LoanAmount { get; init; }
        public decimal? SumCost { get; init; }
        public short? InstallmentCount { get; init; }
        public string? ContractFile { get; init; }
        public decimal? CbTrackingCode { get; init; }
    }

}
