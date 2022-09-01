namespace Fastjob.Core.JobProcessor;

public interface IJobProcessorFactory
{
    IJobProcessor New();
}