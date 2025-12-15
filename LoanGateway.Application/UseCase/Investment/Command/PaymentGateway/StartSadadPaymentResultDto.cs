namespace LoanService.Application.UseCase.Investment.Command.PaymentGateway
{
    public class StartSadadPaymentResultDto
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public  decimal AmountRials { get; set; }
        public  string TokenMasked { get; set; }
        public string RedirectUrl { get; set; } = default!;
    }
}
