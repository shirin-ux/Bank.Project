using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common;

public sealed record Error(string Code, string Message)
{
    public static Error Provider(string provider, string message)
          => new($"{provider.ToUpperInvariant()}_ERROR", message);
}
