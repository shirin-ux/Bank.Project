namespace LoanService.Api.Dtos
{
    public sealed class StartLoanRequestDto
    {
        public string Provider { get; set; } = "Mellat"; 
        public string ProductCode { get; set; }         
        public long Amount { get; set; }              
        public string NationalId { get; set; }         
        public string Mobile { get; set; }            
        public string? MerchantId { get; set; }         
        public bool? WithCollateral { get; set; }       
        public string? IdempotencyKey { get; set; }     
    }
}
