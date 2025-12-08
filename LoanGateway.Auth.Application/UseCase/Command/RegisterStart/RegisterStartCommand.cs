using Common;
using MediatR;

namespace LoanGateway.Auth.Application.UseCase.Command.RegisterStart;
public sealed record RegisterStartCommand(string MobileNumber, string NationalCode) : IRequest<Result<RegisterStartResultDto>>;
