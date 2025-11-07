using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public sealed class BankCodeRule
    {
        public bool IsSuccess { get; set; } = default!;
        public string? NextState { get; set; } = default!;
        public string? UiMessage { get; set; } = default!;
        public bool? Retryable { get; set; } = false;
        public string? Severity { get; set; } = "Info";
    }
}
