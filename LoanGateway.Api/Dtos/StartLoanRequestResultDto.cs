namespace LoanService.Api.Dtos
{
    public sealed class StartLoanRequestResultDto
    {
        public Guid LoanRequestId { get; set; }
        public string State { get; set; }               
        public string Provider { get; set; }             
        public string ProductCode { get; set; }
        public IEnumerable<string> NextActions { get; set; } = Array.Empty<string>();
   
    }
}
