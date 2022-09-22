namespace YAJQ.Core.Common;

/// <summary>
/// Value that indicates the outcome of an operation.
/// </summary>
/// <typeparam name="TValue">The type of the operation or <see cref="Success"/> if it does not return anything</typeparam>
public record struct ExecutionResult<TValue>
{
    public ExecutionResult(TValue value)
    {
        Value = value;
        Error = default;
        WasSuccess = true;
    }

    public ExecutionResult(Error error)
    {
        Value = default;
        Error = error;
        WasSuccess = false;
    }

    public Error Error { get; }
    public TValue Value { get; }
    public bool WasSuccess { get; }

    public static ExecutionResult<Success> Success => new(new Success());

    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onError)
    {
        return WasSuccess ? onSuccess(Value) : onError(Error);
    }


    public static implicit operator ExecutionResult<TValue>(TValue value)
    {
        return new(value);
    }

    public static implicit operator ExecutionResult<TValue>(Error error)
    {
        return new(error);
    }
}

/// <summary>
/// Represents a successful execution
/// </summary>
public readonly record struct Success;