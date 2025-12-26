using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class RateLimitOptions
    {
        public int MaxRequests { get; set; } = 10; 
        public int WindowSeconds { get; set; } = 60; 
    }
}
