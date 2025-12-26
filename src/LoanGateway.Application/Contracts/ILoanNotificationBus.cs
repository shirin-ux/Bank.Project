using LoanService.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.Contracts;

public interface ILoanNotificationBus
{
    Task PublishAsync(LoanNotificationMessage message, CancellationToken ct);
}
