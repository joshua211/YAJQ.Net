using YAJQ.Core.JobHandler.Interfaces;
using YAJQ.Core.JobProcessor;

namespace YAJQ.Core;

/// <summary>
/// Option class to configure YAJQ
/// </summary>
public class YAJQOptions
{
    /// <summary>
    /// The time (in ms) the <see cref="IJobHandler"/> sleeps until he checks the database for a new job
    /// </summary>
    public int HandlerTimeout { get; set; } = 1000;

    /// <summary>
    /// The number of parallel executions allowed on each <see cref="IJobHandler"/>
    /// </summary>
    public int NumberOfProcessors { get; set; } = 10;

    /// <summary>
    /// The base time (in ms) that is delayed until the <see cref="DefaultTransientFaultHandler"/> tries its next execution
    /// </summary>
    public int TransientFaultBaseDelay { get; set; } = 100;

    /// <summary>
    /// The maximum number of times the <see cref="DefaultTransientFaultHandler"/> will try to execute a method
    /// </summary>
    public int TransientFaultMaxTries { get; set; } = 5;

    /// <summary>
    /// The time limit (in seconds) until a job is considered overdue and another handler can take over
    /// </summary>
    public int MaxOverdueTimeout { get; set; } = 5;

    /// <summary>
    /// The time (in ms) the <see cref="IScheduledJobSubHandler"/> waits after each round of execution
    /// </summary>
    public int ScheduledJobTimerInterval { get; set; } = 1000;

    /// <summary>
    ///     The time (in ms) to wait if the same job is processed again (Only one job in the database)
    /// </summary>
    public int SameJobThrottling { get; set; } = 100;
}