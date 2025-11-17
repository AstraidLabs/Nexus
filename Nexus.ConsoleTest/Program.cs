using System.Runtime.Versioning;
using System.Text;
using Nexus;
using Nexus.Core.Abstractions;
using Nexus.Core.Services;

namespace Nexus.ConsoleTest;

[SupportedOSPlatform("windows")]
internal class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            var facade = CreateFacade();
            var diagnostics = new NexusDiagnostics(facade);

            if (!OperatingSystem.IsWindows())
            {
                Console.Error.WriteLine("Běh mimo Windows: zobrazuji pouze ukázková data.");
            }

            Console.WriteLine(diagnostics.BuildReport());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Nepodařilo se načíst informace z knihovny Nexus.");
            Console.Error.WriteLine(ex);
        }
    }

    private static IWindowsInfoFacade CreateFacade()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsInfoFacade();
        }

        return new NonWindowsInfoFacade();
    }
}
