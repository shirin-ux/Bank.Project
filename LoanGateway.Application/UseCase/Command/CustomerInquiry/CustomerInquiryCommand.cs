using Common;
using LoanService.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.CustomerInquiry
{
    public sealed record CustomerInquiryCommand : IRequest<CustomerInquiryResultDto>
    {
        public BankProviderType ProviderType { get; set; }
        public string NationalCode { get; init; } = default!;
        public short? ConfigType { get; init; }    
        public string? BirthDate { get; init; } 
        public string? MobileNo { get; init; }  
        public string? PostalCode { get; init; }  
        public decimal? RequestAmount { get; init; }
        public decimal? ApprovalCode { get; init; }
        public decimal? CbTrackingCode { get; init; }
    
    }
}
