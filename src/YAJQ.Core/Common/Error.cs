namespace YAJQ.Core.Common;

/// <summary>
/// A struct to represent a error in this library
/// </summary>
public struct Error
{
    public string Description { get; }
    public int Code { get; }

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
    
    public override int GetHashCode()
    {
        return Code;
    }

    public static Error Unexpected()
    {
        return new("An unexpected error has occured", 1);
    }

    public static Error NotFound()
    {
        return new("The requested Job was not found", 404);
    }

    public static Error AlreadyMarked()
    {
        return new("The requested Job has already been marked", 2);
    }

    public static Error ModuleNotLoaded()
    {
        return new("The required module was not loaded", 3);
    }

    public static Error TypeNotFound()
    {
        return new("The required Type was not found in the current assemblies", 4);
    }

    public static Error MethodBaseNotFound()
    {
        return new("The required methodbase could not be constructed", 5);
    }

    public static Error ServiceNotFound(string name)
    {
        return new($"The required service {name} was not found in the ServiceCollection", 6);
    }

    public static Error ExecutionFailed()
    {
        return new("Failed to execute the job", 500);
    }

    public static Error InvalidDelegate()
    {
        return new("The provided delegate can't be processed", 7);
    }

    public static Error StorageError()
    {
        return new("Something went wrong with the JobStorage", 8);
    }

    public static Error CursorOutOfRange()
    {
        return new("The current cursor does not point to a job", 9);
    }

    public static Error WrongToken()
    {
        return new("The token provided for the refresh is not the same as the job token", 10);
    }

    public static Error OutdatedUpdate()
    {
        return new("The expected value is outdated", 11);
    }
}