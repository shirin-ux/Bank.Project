using LoanService.Domain.Enum;
using MediatR;

namespace LoanService.Application.UseCase.Command.GetCollateralContractFile
{
    public sealed record GetCollateralContractFileCommand : IRequest<GetCollateralContractFileResultDto>
    {
        public BankProviderType ProviderType { get; set; }
        public decimal ApprovalCode { get; init; }
        public string NationalCode { get; init; } = default!;
        public string CollateralDate { get; init; } = default!;
        public decimal CollateralAmount { get; init; } = default!;
        public decimal CollateralNo { get; init; } = default!;
        public string GuarantorNC { get; init; } = default!;
        public string BirthDate { get; init; } = default!;
        public string MobileNumber { get; init; } = default!;
        public string PostalCode { get; init; } = default!;
        public string PhoneNumber { get; init; } = default!;
        public decimal? LoanAmount { get; init; }
        public short InstallmentCount { get; init; }
        public string? Address { get; init; }

        public string? CollateralIssuer { get; init; }
        public string? ChequeSerial { get; init; }
        public CollateralType CollateralType { get; init; }

    }

}
