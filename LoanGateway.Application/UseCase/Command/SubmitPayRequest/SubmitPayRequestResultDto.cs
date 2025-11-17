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
        public string[] NextActions { get; set; }
        public string State { get; set; }

        public string RequestId { get; set; }
        public Dictionary<string, string[]>? Details { get; set; }

        public IEnumerable<BankStatusItem> GetStatusItems()
        {
            throw new NotImplementedException();
        }
    }
}
