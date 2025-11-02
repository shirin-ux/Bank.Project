using Common;
using LoanService.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.CustomerInquiryStatus
{
    public sealed record CustomerInquiryStatusResultDto:IBankResponse
    {
        public bool Allowed { get; init; }
        public decimal? MaxApprovedAmount { get; init; }
        public IReadOnlyList<StatusItemDto> StatusList { get; init; } = Array.Empty<StatusItemDto>();
        public int? Ics { get; init; }
        public Grade? IcsGrade { get; init; }
        public DateTime? RequestExpireDate { get; init; }
        public short? Gender { get; init; }
        public short? PostalCodeStatus { get; init; }

        public string? MessageCode { get; init; }

        public string? Message { get; init; }
        public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ContractBase64 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public sealed record StatusItemDto(string? ResponseCode, string? ResponseStatus);
    }
  
}
