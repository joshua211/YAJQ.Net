namespace YAJQ.Core.Persistence;

public class JobCursor
{
    private JobCursor(int currentCursor, int maxCursor)
    {
        if (currentCursor == maxCursor)
            throw new Exception("Current cursor(index) can't point to max cursor(length)");

        CurrentCursor = currentCursor;
        MaxCursor = maxCursor;
    }

    public int CurrentCursor { get; private set; }
    public int MaxCursor { get; private set; }

    public static JobCursor Empty => new(-1, 0);

    public static JobCursor With(int current, int max)
    {
        return current == 0 && max == 0 ? Empty : new JobCursor(current, max);
    }

    public JobCursor Increase()
    {
        //No max
        if (MaxCursor == 0)
            return Empty;

        //current is at max index
        if (MaxCursor == CurrentCursor + 1)
            return new JobCursor(0, MaxCursor);

        return new JobCursor(CurrentCursor + 1, MaxCursor);
    }

    public JobCursor IncreaseMax()
    {
        return MaxCursor == 0 ? new JobCursor(0, 1) : new JobCursor(CurrentCursor, MaxCursor + 1);
    }

    public JobCursor DecreaseMax()
    {
        return MaxCursor == 1 ? Empty : new JobCursor(CurrentCursor - 1, MaxCursor - 1);
    }

    public override string ToString()
    {
        return $"({CurrentCursor}/{MaxCursor})";
    }
}