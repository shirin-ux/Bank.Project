using Bank.Mellat.Provider.Dtos;
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
        public List<StatusItemDto> StatusList { get; init; } 
        public string? RequestExpireDate { get; init; }
        public short? PostalCodeStatus { get; init; }
        public short Gender { get; set; }
        public int? MessageCode { get; init; }

        public string? Message { get; init; }
        public string[] NextActions { get ; set ; }
        public string State { get; set; }
        public string ContractBase64 { get ; set; }
        public string RequestId { get; set; }

        public sealed record StatusItemDto(string ResponseCode, string ResponseStatus);
    }
  
}
