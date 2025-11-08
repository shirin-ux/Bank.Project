using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.SubmitPayRequest
{
    public class SubmitPayRequestResultDto:IBankResponse
    {
        public string? PayRequestId { get; init; }

        public int? MessageCode { get; set; }

        public string? Message { get; init; }
        public string[] NextActions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ContractBase64 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RequestId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IEnumerable<BankStatusItem> GetStatusItems()
        {
            throw new NotImplementedException();
        }
    }
}
