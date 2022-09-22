namespace YAJQ.Core.Persistence;

/// <summary>
/// Strongly typed Id to identify <see cref="PersistedJob">Persisted Jobs</see>.
/// Defaults to a random <see cref="Guid"/>.
/// </summary>
public class JobId
{
    private JobId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new Exception($"Unexpected job id value: {id}");

        Value = id;
    }

    public string Value { get; }

    public static JobId New => new(Guid.NewGuid().ToString());

    public static JobId With(string id)
    {
        return new(id);
    }

    public static implicit operator string(JobId id)
    {
        return id.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not JobId id)
            return false;

        return id.Value == Value;
    }
    
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }
}