using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shahkar.Provider.Dto
{
   public  class ShahkarMatchRequestDto
    {
        public string businessId { get; set; }
        public string businessToken { get; set; }
        public string nationalId { get; set; }
        public string mobileNumber { get; set; }
    }
}
