namespace Fastjob.Core.Common;

public record struct ExecutionResult<TValue>
{
    public Error Error { get; private set; }
    public TValue Value { get; private set; }
    public bool WasSuccess { get; private set; }

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

    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onError)
    {
        return WasSuccess ? onSuccess(Value) : onError(this.Error);
    }
    

    public static implicit operator ExecutionResult<TValue>(TValue value) => new(value);
    public static implicit operator ExecutionResult<TValue>(Error error) => new(error);

    public static ExecutionResult<Success> Success => new ExecutionResult<Success>(new Success());
}

public readonly record struct Success;