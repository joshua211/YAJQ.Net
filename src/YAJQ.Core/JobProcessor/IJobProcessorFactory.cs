namespace YAJQ.Core.JobProcessor;

public interface IJobProcessorFactory
{
    IJobProcessor New();
}