using System.Collections.Concurrent;

namespace Fastjob.Tests.Shared;

public static class CallReceiver
{
    public static ConcurrentDictionary<string, Call> ReceivedCalls;

    static CallReceiver()
    {
        ReceivedCalls = new ConcurrentDictionary<string, Call>();
    }

    public static TestLogger TestLogger { private get; set; }


    public static void AddCall(string id)
    {
        ReceivedCalls.AddOrUpdate(id, s => new Call(1, DateTimeOffset.Now),
            (s, c) => new Call(c.Number + 1, DateTimeOffset.Now));
    }

    public static bool WasCalledXTimes(string id, int x = 1)
    {
        if (ReceivedCalls.TryGetValue(id, out var call))
        {
            TestLogger.Log($"{id} was called {call.Number} time(s)");
            return call.Number == x;
        }

        TestLogger.Log($"{id} was not called");
        return false;
    }

    public static bool WasCalledAt(string id, DateTimeOffset time)
    {
        if (ReceivedCalls.TryGetValue(id, out var call))
            return call.LastCall.ToUnixTimeSeconds() - time.ToUnixTimeSeconds() < 2;

        return false;
    }
}

public record Call(int Number, DateTimeOffset LastCall);