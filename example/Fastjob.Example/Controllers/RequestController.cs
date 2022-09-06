using Fastjob.Core.JobQueue;
using Microsoft.AspNetCore.Mvc;

namespace Fastjob.Example.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestController : ControllerBase
{
    private readonly IJobQueue jobQueue;
    private readonly IRequestHandler requestHandler;

    public RequestController(IRequestHandler requestHandler, IJobQueue jobQueue)
    {
        this.requestHandler = requestHandler;
        this.jobQueue = jobQueue;
    }

    [HttpGet]
    public async Task<ActionResult<string>> GetRequest([FromQuery] string token)
    {
        var request = requestHandler.GetRequest(token);
        if (request is null)
        {
            await jobQueue.EnqueueJobAsync(() => requestHandler.AddRequestAsync(token));
            return NotFound();
        }

        return $"Your request was completed after {request.Delay}ms!";
    }
}