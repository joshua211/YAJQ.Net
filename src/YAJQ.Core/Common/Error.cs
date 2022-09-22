namespace YAJQ.Core.Common;

public struct Error
{
    public string Description { get; private set; }
    public int Code { get; private set; }

    public Error(string description, int code)
    {
        Description = description;
        Code = code;
    }

    public override string ToString()
    {
        return $"[{Code}] {Description}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Error error)
            return false;

        return error.Code == Code;
    }

    public static Error Unexpected() => new("An unexpected error has occured", 1);
    public static Error NotFound() => new("The requested Job was not found", 404);
    public static Error AlreadyMarked() => new("The requested Job has already been marked", 2);
    public static Error ModuleNotLoaded() => new("The required module was not loaded", 3);
    public static Error TypeNotFound() => new("The required Type was not found in the current assemblies", 4);
    public static Error MethodBaseNotFound() => new("The required methodbase could not be constructed", 5);

    public static Error ServiceNotFound(string name) =>
        new($"The required service {name} was not found in the ServiceCollection", 6);

    public static Error ExecutionFailed() => new("Failed to execute the job", 500);
    public static Error InvalidDelegate() => new("The provided delegate can't be processed", 7);
    public static Error StorageError() => new("Something went wrong with the JobStorage", 8);
    public static Error CursorOutOfRange() => new("The current cursor does not point to a job", 9);

    public static Error WrongToken() => new("The token provided for the refresh is not the same as the job token", 10);

    public static Error OutdatedUpdate() => new("The expected value is outdated", 11);
}