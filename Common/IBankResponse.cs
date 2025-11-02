using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IBankResponse
    {
        string? MessageCode { get; }
        string? Message { get; }
        string[] NextActions { get; set; }
        string State { get; set; }
        string RequestId { get; set; }

    }
}
