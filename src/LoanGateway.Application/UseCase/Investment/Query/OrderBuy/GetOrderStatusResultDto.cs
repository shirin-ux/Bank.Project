using LoanService.Domain.Enum.Investment;

namespace LoanService.Application.UseCase.Investment.Query.OrderBuy
{
    public class GetOrderStatusResultDto
    {
        /// <summary>
        /// وضعیت نهایی اولی (True یعنی هر دو سفارش قطعی انجام شده)
        /// </summary>
        public bool UliStatus { get; set; }


        /// <summary>
        /// کد وضعیت:
        /// 4 = لغو شده
        /// غیر از 4 = در حال پردازش یا سایر حالات
        /// </summary>
        public statusType Status { get; set; }
        
        /// <summary>
        /// شناسه Policy سرمایه (wealthPolicyId) از کاریزما
        /// </summary>
        public long? WealthPolicyId { get; set; }
    }


}
