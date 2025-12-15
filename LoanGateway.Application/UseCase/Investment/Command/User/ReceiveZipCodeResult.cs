using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.User
{
    public class ReceiveZipCodeResult
    {
        public string PostalCode { get; set; }
        public string Message { get; set; }
    }
}
