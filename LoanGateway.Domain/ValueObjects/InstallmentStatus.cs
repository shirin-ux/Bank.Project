namespace LoanService.Domain.ValueObjects
{
    public sealed class InstallmentStatus
    {

        public decimal ContractNumber { get; set; }
        public short InstallmentNo { get; set; }
        public string NationalCode { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; }

    }
}
