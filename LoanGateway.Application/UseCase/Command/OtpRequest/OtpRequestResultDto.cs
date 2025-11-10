using Common;

namespace LoanService.Application.UseCase.Command.OtpRequest
{
    public sealed record OtpRequestResultDto: IBankResponse
    {
        public Guid LoanRequestId { get; set; }


        public string? State { get; set; }           
        public string[] NextActions { get; set; } = Array.Empty<string>();

        public int? MessageCode { get; set; }

        public string? Message { get; set; }
        public string RequestId { get; set; }

        public IEnumerable<BankStatusItem> GetStatusItems()
        {
            throw new NotImplementedException();
        }
    }
}
