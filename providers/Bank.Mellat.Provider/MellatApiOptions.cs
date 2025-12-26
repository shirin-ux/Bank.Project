using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider
{
    public sealed class MellatApiOptions
    {
        public string BaseUrlApi { get; set; } = default!;  
        public string BaseUrlToken { get; set; } = default!;   
        public string GrantType { get; set; } = "password";
        public string ClientId { get; set; } = default!;
        public string ClientSecret { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public int TimeoutSec { get; set; } = 30;
        public bool BypassProxyOnLocal { get; set; } = default!;
        public string Proxy { get; set; } = default!;
    }
}
