namespace LoanService.Domain.Entities.Loan
{
    public sealed class InstallmentStatus
    {
        public Guid Id { get; set; }
        public Guid LoanRequestId { get; set; }
        public decimal ContractNumber { get; set; }
        public short InstallmentNo { get; set; }
        public string NationalCode { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; }

    }
}
