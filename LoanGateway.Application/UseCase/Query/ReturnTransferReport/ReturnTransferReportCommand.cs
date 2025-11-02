using Common;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Query.ReturnTransferReport;

public sealed record ReturnTransferReportCommand(
  BankProviderType ProviderType,
int ReturnDate,          // YYYYMMDD (INTEGER) - اجباری
    decimal FromId           // DECIMAL(18) - اجباری (اولین بار صفر)
) : IRequest<ReturnTransferReportResultDto>;
