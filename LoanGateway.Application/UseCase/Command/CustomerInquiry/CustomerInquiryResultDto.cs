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
    
        public string? MessageCode { get; init; }
        public string? Message { get; init; }
        public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
