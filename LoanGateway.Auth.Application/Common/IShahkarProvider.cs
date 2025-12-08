using LoanGateway.Auth.Application.UseCase.Command.MobileOwner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.Common
{
   public interface IShahkarProvider
    {
        Task<VerifyMobileOwnerResultDto> VerifyMobileOwnerAsync(VerifyMobileOwnerCommand cmd, CancellationToken cancellationToken = default);
    }
}
