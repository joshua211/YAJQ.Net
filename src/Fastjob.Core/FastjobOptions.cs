namespace Fastjob.Core;

public class FastjobOptions
{
    public int HandlerTimeout { get; set; } = 1000;
    public int NumberOfProcessors { get; set; } = 10;
}