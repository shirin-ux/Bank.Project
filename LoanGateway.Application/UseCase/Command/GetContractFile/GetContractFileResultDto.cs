using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetContractFile
{
    public sealed record GetContractFileResultDto:IBankResponse
    {
        public string ContractPath{ get; init; } = default!; 
        public byte[] ContractFile{ get; init; } = default!; 
        public decimal ContractNumber { get; init; }               
        public int? MessageCode { get; init; }
        public string? Message { get; init; }
        public string[] NextActions { get; set; }
        public string State { get; set; }
        public string RequestId { get; set; }
    }
}
