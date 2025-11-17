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
            var client = NexusClient.Create(facade, options => options.LogSink = LogToConsole);

            if (!OperatingSystem.IsWindows())
            {
                Console.Error.WriteLine("Běh mimo Windows: zobrazuji pouze ukázková data.");
            }

            var snapshot = client.CaptureSnapshot();
            var presenter = new NexusSnapshotPresenter();
            var presentation = presenter.Prepare(snapshot);

            Console.WriteLine(presentation.ToConsoleString());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Nepodařilo se načíst informace z knihovny Nexus.");
            Console.Error.WriteLine(ex);
        }
    }

    private static void LogToConsole(NexusLogEntry entry)
    {
        var prefix = entry.Level switch
        {
            NexusLogLevel.Error => "[Chyba]",
            NexusLogLevel.Warning => "[Varování]",
            _ => "[Info]"
        };

        Console.Error.WriteLine($"{prefix} {entry.Scope}: {entry.Message}");
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
