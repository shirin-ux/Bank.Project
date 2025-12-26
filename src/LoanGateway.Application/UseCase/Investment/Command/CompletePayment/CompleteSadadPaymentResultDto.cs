using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.CompletePayment
{
    public sealed record CompleteSadadPaymentResultDto(
     Guid PaymentId,
     decimal OrderId,
     bool Success,
     int ResCode,
     string Message,
     string? RetrievalRefNo,
     string? SystemTraceNo
 );
}
