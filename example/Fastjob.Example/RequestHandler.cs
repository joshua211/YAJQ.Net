namespace Fastjob.Example;

public interface IRequestHandler
{
    Task<Request?> GetOrStartRequestAsync(string token);
    Task ClearRequests();
}

public class RequestHandler
{
}