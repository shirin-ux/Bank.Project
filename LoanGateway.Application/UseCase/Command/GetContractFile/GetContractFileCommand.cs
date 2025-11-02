

using Common;
using LoanService.Domain.Enum;
using MediatR;


namespace LoanService.Application.UseCase.Command.GetContractFile
{
    public sealed record GetContractFileCommand : IRequest<GetContractFileResultDto>
    {
        public BankProviderType ProviderType { get; set; }
        public decimal ApprovalCode { get; init; }        
        public string NationalCode { get; init; } = default!;
        public string BirthDate { get; init; } = default!;  
        public string MobileNumber { get; init; } = default!;
        public string PostalCode { get; init; } = default!;   
        public string PhoneNumber { get; init; } = default!;  
        public decimal? LoanAmount { get; init; }             
        public short InstallmentCount { get; init; }      
        public string? Address { get; init; }           
        public string? CbTrackingCode { get; init; }         
    }

}
