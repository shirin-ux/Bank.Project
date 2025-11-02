using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public Error? Error { get; }

        public static Result Success() => new(true, null);
        public static Result Failure(Error error) => new(false, error);

        protected Result(bool success, Error? error)
        {
            IsSuccess = success;
            Error = error;
        }
    }
}
