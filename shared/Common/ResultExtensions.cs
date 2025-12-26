using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common;

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool success, T? value, Error? error)
        : base(success, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static new Result<T> Failure(Error error) => new(false, default, error);
}
