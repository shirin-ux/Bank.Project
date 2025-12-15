using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadadProvider
{
    public sealed class SadadOptions
    {
        public string MerchantId { get; init; } = default!;
        public string TerminalId { get; init; } = default!;
        public string TerminalKeyBase64 { get; init; } = default!; 
        public string ReturnUrl { get; init; } = default!;

        public string PaymentRequestUrl { get; init; } = "https://sadad.shaparak.ir/VPG/api/v0/Request/PaymentRequest";
        public string VerifyUrl { get; init; } = "https://sadad.shaparak.ir/VPG/api/v0/Advice/Verify";
        public string PurchaseBaseUrl { get; init; } = "https://sadad.shaparak.ir/VPG/Purchase";
    }
}
