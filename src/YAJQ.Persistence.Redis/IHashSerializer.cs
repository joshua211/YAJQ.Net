using StackExchange.Redis;
using YAJQ.Core.Persistence;

namespace YAJQ.Persistence.Redis;

public interface IHashSerializer
{
    HashEntry[] Serialize(PersistedJob job);
    PersistedJob Deserialize(HashEntry[] hash);
    
    HashEntry[] SerializeCursor(JobCursor cursor);
    JobCursor DeserializeCursor(HashEntry[] hash);
}