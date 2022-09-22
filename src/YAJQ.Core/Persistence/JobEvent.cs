namespace YAJQ.Core.Persistence;

public class JobEvent
{
    public JobEvent(JobId jobId, JobState state)
    {
        JobId = jobId;
        State = state;
    }

    public JobId JobId { get; }
    public JobState State { get; }
}