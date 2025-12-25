using LoanService.Application.UseCase.Investment.Command.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.Contracts
{
    public interface IUserReadService
    {
        Task<UserCommand?> UpdateUserAsync(Guid UserId,string PostalCode, CancellationToken ct);
        Task<UserInfoDto?> GetUserByIdAsync(Guid userId, CancellationToken ct);
    }

    public class UserInfoDto
    {
        public Guid UserId { get; set; }
        public string? NationalCode { get; set; }
        public string? PostalCode { get; set; }
        public string? BirthDate { get; set; }
        public string? MobileNumber { get; set; }
        public bool IsActive { get; set; }
        public bool? GiftStatus { get; set; }
    }
}
