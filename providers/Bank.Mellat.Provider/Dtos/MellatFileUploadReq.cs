using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
    public class MellatFileUploadReq
    {
        public string nationalCode { get; set; }
        public string birthDate { get; set; }
        public string mobileNumber { get; set; }
        public int approvalCode { get; set; }
        public decimal postalCode { get; set; }
        public string phoneNumber { get; set; }
        public decimal loanAmount { get; set; }
        public short installmentCount { get; set; }
        public string? address { get; set; }
        public string cbTrackingCode { get; set; }
    }
}
