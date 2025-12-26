using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Query.CheckUser
{
    public class CheckUserExistsQuery : IRequest<bool>
    {
        public Guid UserId { get; set; }
    }
}
