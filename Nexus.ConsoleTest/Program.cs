using System.Runtime.Versioning;
using System.Text;
using Nexus;

namespace Nexus.ConsoleTest;

[SupportedOSPlatform("windows")]
internal class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            var diagnostics = new NexusDiagnostics();
            var report = diagnostics.BuildReport();
            Console.WriteLine(report);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Nepodařilo se načíst informace z knihovny Nexus.");
            Console.Error.WriteLine(ex);
        }
    }
}
