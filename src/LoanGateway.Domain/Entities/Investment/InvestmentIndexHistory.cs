using LoanService.Domain.Enum.Investment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Entities.Investment
{
    public class InvestmentIndexHistory
    {
        public InvestmentPlanType PlanType { get; set; }  
        public DateTime IndexDateTimeUtc { get; set; }                
        public decimal IndexValue { get; set; }
    }
}
