using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Query.PayResponse;

public sealed record GetPayResponseQuery
    : IRequest<GetPayResponseResultDto>
{
    public string PayRequestId { get; set; }
    public ProviderType ProviderType { get; set; }
};
