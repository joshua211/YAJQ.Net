using YAJQ.Core.Persistence;

namespace YAJQ.Core.JobHandler;

public interface IOpenJobProvider
{
    Task<PersistedJob> GetNextJobAsync(string handlerId, CancellationTokenSource wakeupTokenSource,
        CancellationToken cancellationToken = default);
}