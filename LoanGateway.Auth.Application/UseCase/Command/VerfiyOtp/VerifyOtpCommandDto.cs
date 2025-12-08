using Common;
using LoanGateway.Auth.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.VerfiyOtp
{
  public sealed  class VerifyOtpCommandDto:IRequest<Result<VerifyOtpResultDto>>
    {
        public string PhoneNumber { get; init; } = default!;
        public OtpPurpose Purpose { get; init; }
        public string Code { get; init; } = default!;
        public string? RequestIp { get; init; }
    }
}
