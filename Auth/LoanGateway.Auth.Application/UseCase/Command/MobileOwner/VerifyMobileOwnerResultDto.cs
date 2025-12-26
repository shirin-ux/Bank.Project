using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.MobileOwner
{
  public  class VerifyMobileOwnerResultDto
    {
        public bool IsMatched { get; init; }
        public int StatusCode { get; init; }
        public string? StatusMessage { get; init; }
        public string? RequestId { get; init; }

        public string? CorrelationId { get; init; }
    }

}
