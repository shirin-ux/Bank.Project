using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common;

public sealed record Error(int? Code, string Message)
{
    public static Error Provider(string provider, string message)
          => new(-1, message);
}
