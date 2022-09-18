using Fastjob.Core.Common;
using Fastjob.Core.Persistence;

namespace Fastjob.Core.Archive;

public interface IJobArchive
{
    Task<ExecutionResult<Success>> AddToArchiveAsync(ArchivedJob job);
    Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetCompletedJobsAsync();
    Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetArchivedJobsAsync();
    Task<ExecutionResult<IEnumerable<ArchivedJob>>> GetFailedJobsAsync();
}