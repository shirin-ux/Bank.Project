using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Enum.Investment
{
    public enum statusType : int
    {
        [Display(Name ="انجام شد")]
        FinalStatus = 3,
        [Display(Name = "در حال انجام ")]
        ProcessingStatus = 1,
        [Display(Name = "انجام نشد")]
        CancelledStatus = 4
    }
}
