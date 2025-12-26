using Newtonsoft.Json;

namespace LoanGateway.Auth.Application.UseCase.Command.CompleteProfile
{
    public class ComplateProfileResultDto
    {
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string NationalCode { get; set; } = default!;
        public string BirthDate { get; set; }
        public string ShenasnameSerial{get;set;}
        public string ShenasnameSeri { get;set;}
        public string ShenasnamehNumber { get;set;}
        public bool IsProfileCompleted { get; set; }

        public string AccessToken { get; set; } = default!;
        public DateTime AccessTokenExpiresAtUtc { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; } = default!;
        public DateTime RefreshTokenExpiresAtUtc { get; set; }
    }
}
