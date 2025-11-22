using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Query.GetInstallments;

public sealed record GetInstallmentsQuery : IRequest<GetInstallmentsResultDto>
{
    public string NationalCode { get; set; }
    public string ContractNumber { get; set; }
    public ProviderType ProviderType { get; set; }
}