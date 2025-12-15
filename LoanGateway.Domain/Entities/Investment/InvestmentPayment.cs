using LoanService.Domain.Enum;
using LoanService.Domain.Enum.Investment;

namespace LoanService.Domain.Entities.Investment
{
    public class InvestmentPayment : BaseEntity
    {
        public Guid OrderId { get;  set; }
        public decimal AmountRials { get;  set; }
        public PaymentStatus Status { get;  set; }
        public bool IsFinal { get; set; }

        public int? CallbackResCode { get; set; }
        public int? VerifyResCode { get; set; }

        //public InvestmentPayment(Guid id, Guid orderId, int amountRials)
        //{
        //    if (amountRials <= 0) throw new ArgumentOutOfRangeException(nameof(amountRials));
        //    Id = id;
        //    OrderId = orderId;
        //    AmountRials = amountRials;
        //    Status = PaymentStatus.Created;
        //    IsFinal = false;
        //}

        public void MarkTokenIssued() => Status = PaymentStatus.TokenIssued;

        public void MarkCallbackReceived(int resCode)
        {
            CallbackResCode = resCode;
            Status = PaymentStatus.CallbackReceived;
        }

        public void MarkVerified() { Status = PaymentStatus.Verified; IsFinal = true; }

        public void MarkFailed() { Status = PaymentStatus.Failed; IsFinal = true; }
    }
}

