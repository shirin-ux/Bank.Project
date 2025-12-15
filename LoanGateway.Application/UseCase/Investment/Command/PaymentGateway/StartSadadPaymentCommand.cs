using Common;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.PaymentGateway;

public sealed record StartSadadPaymentCommand(
    Guid OrderId,
    decimal AmountRials
) :IRequest<Result<StartSadadPaymentResultDto>>;
