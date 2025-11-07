using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.CustomerInquiry
{
    public sealed record CustomerInquiryResultDto:IBankResponse
    {
    
        public int? MessageCode { get; init; }
        public string? Message { get; init; }
        public string[]? NextActions { get; set; }
        public string? State { get; set; }
        public string RequestId { get; set; }
    }
}
