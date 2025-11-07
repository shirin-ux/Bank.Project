using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
  public  class MellatInquiryRegisterReq
    {
        public string nationalCode { get; set; }
        public string birthDate { get; set; }
        public string requestAmount { get; set; }
        public short? configType { get; set; }
        public string? postalCode { get; set; }
        public decimal? approvalCode { get; set; }
        public decimal? cbTrackingCode { get; set; }
        public string mobileNo { get; set; }
     

    }
}
