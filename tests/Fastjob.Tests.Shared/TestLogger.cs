using Xunit.Abstractions;

namespace Fastjob.Tests.Shared;

public class TestLogger
{
    private readonly ITestOutputHelper helper;

    public TestLogger(ITestOutputHelper helper)
    {
        this.helper = helper;
    }

    public void Log(string message)
    {
        try
        {
            helper.WriteLine(message);
        }
        catch
        {
        }
    }
}