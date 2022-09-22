namespace YAJQ.Core.Persistence;

public class JobCursor
{
    private JobCursor(int currentCursor, int maxCursor)
    {
        CurrentCursor = currentCursor;
        MaxCursor = maxCursor;
    }

    public int CurrentCursor { get; private set; }
    public int MaxCursor { get; private set; }

    public static JobCursor Empty => new JobCursor(0, 0);

    public JobCursor Increase()
    {
        if (MaxCursor == 1)
            return new JobCursor(1, MaxCursor);
        if (MaxCursor == 0)
            return Empty;
        if (MaxCursor == CurrentCursor)
            return new JobCursor(1, MaxCursor);

        return new JobCursor(++CurrentCursor, MaxCursor);
    }

    public JobCursor IncreaseMax()
    {
        return MaxCursor == 0 ? new JobCursor(1, 1) : new JobCursor(CurrentCursor, ++MaxCursor);
    }

    public JobCursor DecreaseMax()
    {
        if (MaxCursor == 1)
            return JobCursor.Empty;

        return CurrentCursor == MaxCursor
            ? new JobCursor(--CurrentCursor, --MaxCursor)
            : new JobCursor(CurrentCursor, --MaxCursor);
    }

    public override string ToString()
    {
        return $"({CurrentCursor}/{MaxCursor})";
    }
}