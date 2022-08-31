using System.Collections.Concurrent;

namespace Fastjob.Example;

public interface IRequestHandler
{
    Request? GetRequest(string token);

    void ClearRequests();

    Task AddRequestAsync(string token);
}

public class RequestHandler : IRequestHandler
{
    private static Random random = new Random();
    private readonly ConcurrentDictionary<string, Request> requests;

    public RequestHandler()
    {
        requests = new ConcurrentDictionary<string, Request>();
        
    }

    public Request? GetRequest(string token)
    {
        requests.TryGetValue(token, out var request);

        return request;
    }

    public void ClearRequests()
    {
        requests.Clear();
    }

    public async Task AddRequestAsync(string token)
    {
        var delay = random.Next(1000);
        await Task.Delay(delay);

        requests[token] = new Request(delay);
    }
}