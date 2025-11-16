using System.Collections.Generic;
using System.Text;
using Nexus.Core.Models;
using Nexus.Core.Services;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("Nexus – testovací konzole");
Console.WriteLine(new string('=', 60));

var facade = new WindowsInfoFacade();
var digitalLicenseChecker = new DigitalLicenseChecker();

RunSection("Informace o systému", facade.GetSystem, PrintSystem);
RunSection("KMS klient", facade.GetKmsClient, PrintKmsClient);
RunSection("ADBA", facade.GetAdba, PrintAdba);
RunSection("AVMA", facade.GetAvma, PrintAvma);
RunSection("Předplatné", facade.GetSubscription, PrintSubscription);
RunSection("SKU záznamy", facade.GetSkus, PrintSkus);
RunDigitalLicense();

Console.WriteLine();
Console.WriteLine("Hotovo. Stiskni Enter pro ukončení…");
Console.ReadLine();

void RunSection<T>(string title, Func<T> loader, Action<T> renderer)
{
    Console.WriteLine($"\n=== {title} ===");
    try
    {
        var data = loader();
        if (data is null)
        {
            Console.WriteLine("  (Žádná data – operace není podporována nebo neproběhla úspěšně.)");
            return;
        }

        renderer(data);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Chyba: {ex.Message}");
    }
}

void RunDigitalLicense()
{
    Console.WriteLine("\n=== Digitální licence ===");
    try
    {
        var hasLicense = digitalLicenseChecker.IsDigitalLicensePresent();
        var status = hasLicense switch
        {
            true => "Ano",
            false => "Ne",
            null => "Nelze zjistit"
        };
        PrintKeyValue("Přítomnost", status);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Chyba: {ex.Message}");
    }
}

static void PrintSystem(WindowsSystemData data)
{
    PrintKeyValue("Edice", data.Edition);
    PrintKeyValue("Verze jádra", data.Version);
    PrintKeyValue("Architektura", data.Architecture);

    Console.WriteLine("\n  Produkt:");
    PrintKeyValue("    Kód typu", data.Product.ProductTypeCode.ToString());
    PrintKeyValue("    Název typu", data.Product.ProductTypeName);
    PrintKeyValue("    Release", data.Product.Release);

    Console.WriteLine("\n  Aktivace:");
    PrintActivation(data.Activation);
}

static void PrintActivation(ActivationInfo info)
{
    PrintKeyValue("    Stav", info.Status.ToString());
    PrintKeyValue("    Detaily", info.StatusMessage);
    PrintKeyValue("    Genuine", info.Genuine.ToString());
    PrintKeyValue("    Grace (min)", info.GraceMinutesRemaining?.ToString());
    PrintKeyValue("    Konec evaluace", info.EvaluationEndUtc?.ToString("u"));
    PrintKeyValue("    Kanál", info.Channel);
    PrintKeyValue("    Část klíče", info.PartialProductKey);
    PrintKeyValue("    VL typ", info.VlActivationType?.ToString());
    PrintKeyValue("    Extended PID", info.ExtendedPid);
    PrintKeyValue("    Product ID", info.ProductId);
}

static void PrintKmsClient(KmsClientInfo info)
{
    PrintKeyValue("CMID", info.ClientMachineId);
    PrintKeyValue("Registrovaný KMS host", info.RegisteredKmsName);
    PrintKeyValue("Registrovaný port", info.RegisteredKmsPort?.ToString());
    PrintKeyValue("Detekovaný KMS host", info.DiscoveredKmsName);
    PrintKeyValue("Detekovaný port", info.DiscoveredKmsPort?.ToString());
    PrintKeyValue("Detekovaná IP", info.DiscoveredKmsIp);
    PrintKeyValue("Interval aktivace (min)", info.ActivationIntervalMinutes?.ToString());
    PrintKeyValue("Interval obnovy (min)", info.RenewalIntervalMinutes?.ToString());
    PrintKeyValue("Lookup doména", info.LookupDomain);
    PrintKeyValue("Cache hosta", info.HostCachingEnabled is null ? null : (info.HostCachingEnabled.Value ? "Povolena" : "Zakázána"));
}

static void PrintAdba(AdbaInfo info)
{
    PrintKeyValue("Objekt", info.ObjectName);
    PrintKeyValue("DN", info.ObjectDn);
    PrintKeyValue("CSVLK PID", info.CsvlkPid);
    PrintKeyValue("CSVLK SKU", info.CsvlkSkuId);
}

static void PrintAvma(AvmaInfo info)
{
    PrintKeyValue("Host", info.HostMachineName);
    PrintKeyValue("Host DigitalPID2", info.HostDigitalPid2);
    PrintKeyValue("Inherited Activation ID", info.InheritedActivationId);
    PrintKeyValue("Čas aktivace", info.ActivationTime?.ToString("u"));
}

static void PrintSubscription(SubscriptionInfo info)
{
    PrintKeyValue("Podpora", info.Supported ? "Ano" : "Ne");
    PrintKeyValue("Aktivní", info.Enabled ? "Ano" : "Ne");
    PrintKeyValue("SKU", info.Sku?.ToString());
    PrintKeyValue("Stav", info.State?.ToString());
}

static void PrintSkus(IReadOnlyList<SlSkuEntry> skus)
{
    if (skus.Count == 0)
    {
        Console.WriteLine("  (Nebyly nalezeny žádné SKU záznamy.)");
        return;
    }

    for (var i = 0; i < skus.Count; i++)
    {
        var sku = skus[i];
        Console.WriteLine($"  [{i + 1}] {sku.Name ?? sku.SkuId.ToString()}");
        PrintKeyValue("    Id", sku.SkuId.ToString());
        PrintKeyValue("    Popis", sku.Description);
        PrintKeyValue("    Kanál", sku.Channel);
        PrintKeyValue("    Část klíče", sku.PartialProductKey);
        Console.WriteLine();
    }
}

static void PrintKeyValue(string label, string? value)
{
    Console.WriteLine($"  {label,-24}: {value ?? "<nezjištěno>"}");
}
