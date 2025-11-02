using Common;


namespace LoanService.Application.UseCase.Command.OtpRequest
{
    public sealed record OtpRequestResultDto: IBankResponse
    {
        public Guid LoanRequestId { get; set; }


        public string? State { get; set; }           
        public string[] NextActions { get; set; } = Array.Empty<string>();

        public string? MessageCode { get; set; }

        public string? Message { get; set; }
        public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
