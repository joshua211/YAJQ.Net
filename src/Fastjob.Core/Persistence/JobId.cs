namespace Fastjob.Core.Persistence;

public class JobId
{
    public string Value { get; private set; }

    private JobId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new Exception($"Unexpected job id value: {id}");

        Value = id;
    }

    public static JobId New => new JobId(Guid.NewGuid().ToString());
    public static JobId With(string id) => new JobId(id);

    public static implicit operator string(JobId id) => id.Value;

    public override bool Equals(object? obj)
    {
        if (obj is not JobId id)
            return false;

        return id.Value == Value;
    }
}