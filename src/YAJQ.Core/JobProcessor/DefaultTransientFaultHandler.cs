using Microsoft.Extensions.Logging;
using YAJQ.Core.JobProcessor.Interfaces;

namespace YAJQ.Core.JobProcessor;

public class DefaultTransientFaultHandler : ITransientFaultHandler
{
    private readonly ILogger<DefaultTransientFaultHandler> logger;
    private readonly YAJQOptions options;

    public DefaultTransientFaultHandler(ILogger<DefaultTransientFaultHandler> logger, YAJQOptions options)
    {
        this.logger = logger;
        this.options = options;
    }

    public TransientFaultResult Try(Action action)
    {
        var baseDelay = options.TransientFaultBaseDelay;
        var maxTries = options.TransientFaultMaxTries;
        var numOfTries = 0;
        var shouldTryAgain = true;

        do
        {
            try
            {
                action();
                shouldTryAgain = false;
            }
            catch (Exception e)
            {
                logger.LogDebug(e, "Error while trying to process action");
                numOfTries++;
                if (numOfTries >= maxTries)
                    return new TransientFaultResult(false, e);

                Thread.Sleep(baseDelay * numOfTries);
            }
        } while (shouldTryAgain);

        return new TransientFaultResult(true, null);
    }
}