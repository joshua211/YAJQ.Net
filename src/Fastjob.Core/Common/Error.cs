namespace Fastjob.Core.Common;

public struct Error
{
    public string Description { get; private set; }

    public Error(string description)
    {
        Description = description;
    }

    public static Error AlreadyMarked() => new Error("The requested Job has already been marked");
    public static Error NotFound() => new Error("The requested Job was not found");
}