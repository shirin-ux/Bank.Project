using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Command.GetCollateralContractFile
{
    public sealed record GetCollateralContractFileResultDto:IBankResponse
    {
        public string ContractFile { get; init; }
        public decimal ContractNumber { get; init; }       
        public string? MessageCode { get; init; }
        public string? Message { get; init; }
        public string[] NextActions { get; set; }
        public string State { get; set; }
        public string ContractBase64 { get; set; }
        public string RequestId { get; set; }
    }
}
