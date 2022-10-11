using YAJQ.Core.Archive.Interfaces;
using YAJQ.Core.Common;
using YAJQ.Core.Persistence;

namespace YAJQ.Persistence.Redis.Archive;

public class RedisArchive : IJobArchive
{
    public async Task<ExecutionResult<Success>> AddToArchiveAsync(ArchivedJob job)
    {
        return new Success();
    }

    public Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetCompletedJobsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetArchivedJobsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetFailedJobsAsync()
    {
        throw new NotImplementedException();
    }
}