using System.Diagnostics;

namespace Fastjob.Core.Utils;

public interface IModuleHelper
{
    bool IsModuleLoaded(string moduleName);
}

public class ModuleHelper : IModuleHelper
{
    public bool IsModuleLoaded(string moduleName)
    {
        return Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
            .Any(m => m.ModuleName == moduleName);
    }
}