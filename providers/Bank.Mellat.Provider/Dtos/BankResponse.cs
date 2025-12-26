using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
 public  class BankResponse<T>
    {
        public string ErrorCode { get; set; }   
        public string ErrorMessage { get; set; }
        public T Data { get; set; }      
        public bool ShouldRetry { get; set; }
    }
}
