﻿using System.Collections.Concurrent;

namespace Fastjob.Tests.Shared;

public static class CallReceiver
{
    public static ConcurrentDictionary<string, int> ReceivedCalls;

    static CallReceiver()
    {
        ReceivedCalls = new ConcurrentDictionary<string, int>();
    }

    public static void AddCall(string id)
    {
        ReceivedCalls.AddOrUpdate(id, s => 1, (s, i) => i + 1);
    }

    public static bool WasCalledXTimes(string id, int x = 1)
    {
        if (ReceivedCalls.TryGetValue(id, out var calls))
            return calls == x;

        return false;
    }
}