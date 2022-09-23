using System.Diagnostics;
using System.Reflection;

namespace YAJQ.Core.Utils;

public interface IModuleHelper
{
    bool IsModuleLoaded(string moduleName);
}

public class ModuleHelper : IModuleHelper
{
    public bool IsModuleLoaded(string moduleName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetLoadedModules())
            .Any(m => m.Name == moduleName);
    }
}