using LoanService.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Entities
{    /// <summary>
     /// اقساط و بازپرداخت‌ها
     /// </summary>
     /// <param name="TrackNumber"></param>
     /// <param name="AccountNo"></param>
     /// <param name="Amount"></param>
     /// <param name="WhenUtc"></param>
   public sealed class RepaymentSnapshot
    {

        public Guid Id { get; set; } 
        public Guid LoanRequestId { get; set; } 
        public string? TrackNumber { get; set; }
        public string? AccountNo { get; set; }
        public decimal? Amount { get; set; }
        public Grade? IcsGrade { get; set; }
        public DateTime? WhenUtc { get; set; }
    }
}
