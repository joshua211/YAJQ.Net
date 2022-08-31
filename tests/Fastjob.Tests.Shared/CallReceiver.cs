namespace Fastjob.Tests.Shared;

public static class CallReceiver
{
    public static Dictionary<string, int> ReceivedCalls;

    static CallReceiver()
    {
        ReceivedCalls = new Dictionary<string, int>();
    }

    public static void AddCall(string id)
    {
        if (ReceivedCalls.TryGetValue(id, out var calls))
            ReceivedCalls[id] += 1;
        else
            ReceivedCalls[id] = 1;
    }

    public static bool WasCalledXTimes(string id, int x = 1)
    {
        if (ReceivedCalls.TryGetValue(id, out var calls))
            return calls == x;

        return false;
    }
}