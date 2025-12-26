using Common;
using LoanGateway.Auth.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.RequestOtp
{
  public  class RequestOtpCommandDto:IRequest<Result<RequestOtpResultDto>>
    {
        public string PhoneNumber { get; init; } = default!;
        public OtpPurpose Purpose { get; init; }
        //public string? RequestIp { get; init; }
        //public string? UserAgent { get; init; }
    }
}
