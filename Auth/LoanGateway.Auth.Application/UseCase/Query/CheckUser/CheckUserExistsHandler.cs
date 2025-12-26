using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Query.CheckUser;


public class CheckUserExistsHandler(IUserReadRepository rep) : IRequestHandler<CheckUserExistsQuery, bool>
{
    private readonly IUserReadRepository _rep= rep;

    public async Task<bool> Handle(CheckUserExistsQuery request, CancellationToken cancellationToken)
    {
        var user = await _rep.GetByIdAsync(request.UserId, cancellationToken);
        return user.IsActive;
    }
}
