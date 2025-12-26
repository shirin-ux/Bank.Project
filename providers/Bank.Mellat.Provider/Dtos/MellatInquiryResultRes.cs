

namespace Bank.Mellat.Provider.Dtos
{
   public  class MellatInquiryResultRes
    {
        public bool allowed { get; set; }
        public decimal maxApprovedAmount { get; set; }
        public List<statusList> statusList { get; set; }

        public string requestExpireDate { get; set; }
        public short Gender { get; set; }
        public int ics { get; set; }
        public string icsGrade { get; set; }
        public short postalCode { get; set; }
    }
    public class statusList
    {
        public string responseCode { get; set; }
        public string   responseStatus { get; set; }
    }

 
}
