using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.UpdateUser
{
    using Common;
    using MediatR;

    public class UpdatePostalCodeCommand : IRequest<Result<UpdatePostalCodeResult>>
    {
        public Guid UserId { get; set; }
        public string PostalCode { get; set; } = string.Empty;
    }

    public class UpdatePostalCodeResult
    {
        public Guid UserId { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

}
