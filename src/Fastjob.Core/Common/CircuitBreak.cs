namespace Fastjob.Core.Common;

public class CircuitBreak
{
    private readonly int maxTries;
    private int currentTries;

    public CircuitBreak(int maxTries = 10)
    {
        this.maxTries = maxTries;
        currentTries = 0;
    }

    public bool End { get; private set; }

    public async Task Wait(int waitTimer = 10, CancellationToken cancellationToken = default)
    {
        if (currentTries == maxTries)
        {
            End = true;
            return;
        }

        await Task.Delay(10, cancellationToken);
        currentTries++;
    }
}