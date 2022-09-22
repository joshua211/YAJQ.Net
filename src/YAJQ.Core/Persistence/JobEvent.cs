namespace YAJQ.Core.Persistence;

public class JobEvent
{
    public JobEvent(JobId jobId, JobState state)
    {
        JobId = jobId;
        State = state;
    }

    public JobId JobId { get; private set; }
    public JobState State { get; private set; }
}