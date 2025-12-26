using Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.InvestmentWithdrawal
{
    public sealed record StartWithdrawalCommand(
        Guid InvestmentAccountId,
        decimal Amount,
        string DestinationIban,
        string? Description
    ) : IRequest<Result<StartWithdrawalResultDto>>;
}
