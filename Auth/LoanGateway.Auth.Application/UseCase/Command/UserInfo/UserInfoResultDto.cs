

namespace LoanGateway.Auth.Application.UseCase.Command.UserInfo
{
  public  class UserInfoResultDto
    {
       public string NationalCode { get; set; }
        public string BirthDate { get; set; }
        public string PostalCode  { get; set; }
        public string MobileNumber  { get; set; }
        public string ShenasnameSerial { get; set; }
        public string ShenasnamehNumber { get; set; }
        public string ShenasnameSeri { get; set; }
        public string Address { get; set; }
        public Guid UserId { get; set; }
    }
}
