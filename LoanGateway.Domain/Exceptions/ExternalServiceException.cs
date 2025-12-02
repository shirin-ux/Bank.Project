using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Domain.Exceptions
{
    public class ExternalServiceException : Exception
    {
        public int ExternalStatusCode { get; }
        public string ExternalContent { get; }
        public ExternalServiceException(
             string message,
             int externalStatusCode,
             string externalContent = null,
        Exception? innerException = null)
        : base(message, innerException)
        {
            ExternalStatusCode = externalStatusCode;
            ExternalContent = externalContent;
        }
    }
}
