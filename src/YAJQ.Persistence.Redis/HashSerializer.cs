using System.ComponentModel;
using System.Text.Json;
using StackExchange.Redis;
using YAJQ.Core.JobQueue;
using YAJQ.Core.Persistence;

namespace YAJQ.Persistence.Redis;

public class HashSerializer : IHashSerializer
{
    public HashEntry[] Serialize(PersistedJob job)
    {
        var entries = new HashEntry[8];
        entries[0] = new HashEntry(nameof(job.Id), job.Id.Value);
        entries[1] = new HashEntry(nameof(job.Descriptor), JsonSerializer.Serialize(job.Descriptor));
        entries[2] = new HashEntry(nameof(job.CreationTime), job.CreationTime.ToString("O"));
        entries[3] = new HashEntry(nameof(job.ScheduledTime), job.ScheduledTime.ToString("O"));
        entries[4] = new HashEntry(nameof(job.LastUpdated), job.LastUpdated.ToString("O"));
        entries[5] = new HashEntry(nameof(job.ConcurrencyToken), job.ConcurrencyToken);
        entries[6] = new HashEntry(nameof(job.State), (int) job.State);
        entries[7] = new HashEntry(nameof(job.JobType), (int) job.JobType);

        return entries;
    }

    public PersistedJob Deserialize(HashEntry[] hash)
    {
        var id = JobId.With(hash.First(h => h.Name == nameof(PersistedJob.Id)).Value);
        var descriptor =
            JsonSerializer.Deserialize<JobDescriptor>(hash.First(h => h.Name == nameof(PersistedJob.Descriptor)).Value);
        var crTime = DateTimeOffset.Parse(hash.First(h => h.Name == nameof(PersistedJob.CreationTime)).Value);
        var scheduledTime = DateTimeOffset.Parse(hash.First(h => h.Name == nameof(PersistedJob.ScheduledTime)).Value);
        var lastUpdated = DateTimeOffset.Parse(hash.First(h => h.Name == nameof(PersistedJob.LastUpdated)).Value);
        var token = hash.First(h => h.Name == nameof(PersistedJob.ConcurrencyToken)).Value;
        var state = (JobState) (int) hash.First(h => h.Name == nameof(PersistedJob.State)).Value;
        var type = (JobType) (int) hash.First(h => h.Name == nameof(PersistedJob.JobType)).Value;

        return new PersistedJob(id, descriptor, crTime, scheduledTime, lastUpdated, token, state, type);
    }

    public HashEntry[] SerializeCursor(JobCursor cursor)
    {
        var entries = new HashEntry[2];
        entries[0] = new HashEntry(nameof(cursor.CurrentCursor), cursor.CurrentCursor);
        entries[1] = new HashEntry(nameof(cursor.MaxCursor), cursor.MaxCursor);

        return entries;
    }

    public JobCursor DeserializeCursor(HashEntry[] hash)
    {
        var current = (int) hash.First(h => h.Name == nameof(JobCursor.CurrentCursor)).Value;
        var max = (int) hash.First(h => h.Name == nameof(JobCursor.MaxCursor)).Value;;

        return JobCursor.With(current, max);
    }
}