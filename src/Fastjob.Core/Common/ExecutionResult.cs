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

    public static implicit operator ExecutionResult<TValue>(TValue value) => new (value);
    public static implicit operator ExecutionResult<TValue>(Error error) => new(error);
}

public readonly record struct Success;