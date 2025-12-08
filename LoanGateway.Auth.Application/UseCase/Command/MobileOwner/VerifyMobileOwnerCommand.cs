using Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.MobileOwner
{
  public  class VerifyMobileOwnerCommand:IRequest<Result<VerifyMobileOwnerResultDto>>
    {
        public MobileOwnerRequestContextDto RequestContext { get; set; } = new();
        public string NationalId { get; set; } = default!;
        public string MobileNumber { get; set; } = default!;
    }
    public sealed class MobileOwnerRequestContextDto
    {
        public string BusinessId { get; set; } = default!;
        public string BusinessToken { get; set; } = default!;
    }


}
