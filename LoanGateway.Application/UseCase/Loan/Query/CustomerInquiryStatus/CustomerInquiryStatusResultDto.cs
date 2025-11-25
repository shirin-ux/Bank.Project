using Common;
using LoanService.Domain.Enum.Loan;

namespace LoanService.Application.UseCase.Loan.Query.CustomerInquiryStatus
{
    public sealed record CustomerInquiryStatusResultDto : IBankResponse
    {
        public bool Allowed { get; init; }
        public decimal? MaxApprovedAmount { get; init; }
        public List<StatusItemDto> StatusList { get; init; }
        public string? RequestExpireDate { get; init; }
        public short? PostalCodeStatus { get; init; }
        public short Gender { get; set; }
        public int? MessageCode { get; init; }
        public int Ics { get; set; }
        public Grade IcsGrade { get; set; }
        public string? Message { get; init; }
        public string[] NextActions { get; set; }
        public string State { get; set; }
        public string ContractBase64 { get; set; }
        public string RequestId { get; set; }
        public Dictionary<string, string[]>? Details { get; set; }

        public IEnumerable<BankStatusItem> GetStatusItems()
        {
            foreach (var s in StatusList)
            {
                // s.ResponseCode و s.ResponseStatus توی اسکرین‌شاتت بود
                yield return new BankStatusItem
                {
                    Code = int.TryParse(s.ResponseCode, out var c) ? c : 0,
                    Message = s.ResponseStatus
                };
            }
        }

        public sealed record StatusItemDto(string ResponseCode, string ResponseStatus);
    }

}
