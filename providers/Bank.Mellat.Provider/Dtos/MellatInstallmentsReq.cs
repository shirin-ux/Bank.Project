using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatInstallmentsReq
    {
        public string NationalCode { get; set; } = null!;

        public decimal ContractNumber { get; set; }
    }
}
