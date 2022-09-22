using YAJQ.Core.Common;
using YAJQ.Core.Persistence;

namespace YAJQ.Core.Archive.Interfaces;

public interface IJobArchive
{
    Task<ExecutionResult<Success>> AddToArchiveAsync(ArchivedJob job);
    Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetCompletedJobsAsync();
    Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetArchivedJobsAsync();
    Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetFailedJobsAsync();
}