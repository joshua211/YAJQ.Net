namespace Fastjob.Core.JobProcessor;

public interface ITransientFaultHandler
{
    TransientFaultResult Try(Action action);
}

public record TransientFaultResult(bool WasSuccess, Exception? LastException);