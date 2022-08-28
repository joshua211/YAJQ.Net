namespace Fastjob.Core.Persistence;

public class JobCursor
{
    public int CurrentCursor { get; private set; }
    public int MaxCursor { get; private set; }

    public JobCursor(int currentCursor, int maxCursor)
    {
        CurrentCursor = currentCursor;
        MaxCursor = maxCursor;
    }

    public override string ToString()
    {
        return $"({CurrentCursor}/{MaxCursor})";
    }

    public JobCursor IncreaseMax() => new JobCursor(CurrentCursor, MaxCursor + 1);

    public JobCursor Increase() => CurrentCursor < MaxCursor
        ? new JobCursor(CurrentCursor + 1, MaxCursor)
        : new JobCursor(0, MaxCursor);

    public JobCursor DecreaseMax() => new JobCursor(CurrentCursor, MaxCursor - 1);
}