using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.CompletePayment
{
    public sealed record SadadCallbackDto(
        Guid OrderId,
        string Token,
        int ResCode
    );
}
