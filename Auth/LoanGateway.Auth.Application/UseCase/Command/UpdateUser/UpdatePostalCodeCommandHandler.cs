using Common;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Application.UseCase.Command.UpdateUser;



public class UpdatePostalCodeCommandHandler(IUserReadRepository rep)  : IRequestHandler<UpdatePostalCodeCommand, Result<UpdatePostalCodeResult>>
{
    private readonly IUserReadRepository _rep = rep;


    public async Task<Result<UpdatePostalCodeResult>> Handle(UpdatePostalCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _rep.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("کاربر پیدا نشد.");


        user.PostalCode = request.PostalCode;
        await _rep.UpdateProfileAsync(user, cancellationToken);



        var res= new UpdatePostalCodeResult
        {
            UserId = request.UserId,
            PostalCode = request.PostalCode,
            Message = "کدپستی با موفقیت بروزرسانی شد"
        };
        return Result<UpdatePostalCodeResult>.Success(res);
    }
}
