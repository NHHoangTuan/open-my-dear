using System.Reflection;

namespace OpenMyDear.Wpf.Services;

public sealed class AppVersionService : IAppVersionService
{
    public string GetDisplayVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var infoVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(infoVersion))
        {
            return infoVersion;
        }

        return assembly.GetName().Version?.ToString() ?? "0.0.0";
    }
}
