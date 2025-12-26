using Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.User
{
   public  class ReceiveZipCodeCommand:IRequest<Result<ReceiveZipCodeResult>>
    {
        [Required(ErrorMessage = "کدپستی الزامی است.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کدپستی باید دقیقاً ۱۰ رقم عددی باشد.")]
        public string PostalCode { get; set; }
        public Guid UserId { get; set; }
    }
}
