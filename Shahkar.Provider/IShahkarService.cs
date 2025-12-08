using Shahkar.Provider.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shahkar.Provider
{
    public interface IShahkarService
    {
        Task<ShahkarMatchResponseDto> VerifyMobileOwnerAsync( string nationalId, string mobileNumber,CancellationToken cancellationToken = default);
    }
}
