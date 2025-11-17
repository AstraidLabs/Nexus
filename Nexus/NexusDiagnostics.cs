using Nexus.Core.Abstractions;
using Nexus.Core.Models;
using Nexus.Core.Services;
using System.Runtime.Versioning;
using System.Text;

namespace Nexus;

/// <summary>
/// Pomocník, který přečte všechny dostupné informace z <see cref="WindowsInfoFacade"/>
/// a sestaví srozumitelný textový výstup.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class NexusDiagnostics
{
    private readonly IWindowsInfoFacade _facade;

    public NexusDiagnostics()
        : this(new WindowsInfoFacade())
    {
    }

    public NexusDiagnostics(IWindowsInfoFacade facade)
    {
        _facade = facade ?? throw new ArgumentNullException(nameof(facade));
    }

    /// <summary>
    /// Přečte všechny dostupné informace a vrátí je jako formátovaný text.
    /// </summary>
    public string BuildReport()
    {
        var sb = new StringBuilder();

        AppendSystemInfo(sb, _facade.GetSystem());
        AppendKms(sb, _facade.GetKmsClient());
        AppendAdba(sb, _facade.GetAdba());
        AppendAvma(sb, _facade.GetAvma());
        AppendSubscription(sb, _facade.GetSubscription());
        AppendSkuList(sb, _facade.GetSkus());

        return sb.ToString();
    }

    private static void AppendSystemInfo(StringBuilder sb, WindowsSystemData system)
    {
        sb.AppendLine("=== Informace o systému ===");
        sb.AppendLine($"Edice: {system.Edition}");
        sb.AppendLine($"Verze: {system.Version}");
        sb.AppendLine($"Architektura: {system.Architecture}");
        sb.AppendLine();

        sb.AppendLine("Aktivace:");
        sb.AppendLine($"  Stav: {system.Activation.Status}");
        if (!string.IsNullOrWhiteSpace(system.Activation.StatusMessage))
        {
            sb.AppendLine($"  Popis: {system.Activation.StatusMessage}");
        }

        sb.AppendLine($"  Genuine stav: {system.Activation.Genuine}");

        if (system.Activation.GraceMinutesRemaining is int grace)
        {
            sb.AppendLine($"  Zbývající minuty: {grace}");
        }

        if (system.Activation.EvaluationEndUtc is DateTimeOffset evalEnd)
        {
            sb.AppendLine($"  Platnost zkušební verze do (UTC): {evalEnd:u}");
        }

        if (!string.IsNullOrWhiteSpace(system.Activation.Channel))
        {
            sb.AppendLine($"  Kanál: {system.Activation.Channel}");
        }

        if (!string.IsNullOrWhiteSpace(system.Activation.PartialProductKey))
        {
            sb.AppendLine($"  Částečný produktový klíč: {system.Activation.PartialProductKey}");
        }

        if (system.Activation.VlActivationType is { } volumeType)
        {
            sb.AppendLine($"  Typ aktivace VL: {volumeType}");
        }

        if (!string.IsNullOrWhiteSpace(system.Activation.ExtendedPid))
        {
            sb.AppendLine($"  Extended PID: {system.Activation.ExtendedPid}");
        }

        if (!string.IsNullOrWhiteSpace(system.Activation.ProductId))
        {
            sb.AppendLine($"  Product ID: {system.Activation.ProductId}");
        }

        sb.AppendLine();

        sb.AppendLine("Produkt:");
        sb.AppendLine($"  Typ produktu (kód): {system.Product.ProductTypeCode}");
        sb.AppendLine($"  Typ produktu (název): {system.Product.ProductTypeName}");
        sb.AppendLine($"  Release: {system.Product.Release}");
        sb.AppendLine();
    }

    private static void AppendKms(StringBuilder sb, KmsClientInfo? kms)
    {
        sb.AppendLine("=== KMS klient ===");
        if (kms is null)
        {
            sb.AppendLine("KMS klient není k dispozici.");
            sb.AppendLine();
            return;
        }

        sb.AppendLine($"Client Machine ID: {kms.ClientMachineId}");
        sb.AppendLine($"Registrovaný KMS: {kms.RegisteredKmsName}:{kms.RegisteredKmsPort}");
        sb.AppendLine($"Zjištěný KMS: {kms.DiscoveredKmsName}:{kms.DiscoveredKmsPort}");
        sb.AppendLine($"KMS IP: {kms.DiscoveredKmsIp}");
        sb.AppendLine($"Obnovovací interval (minuty): {kms.RenewalIntervalMinutes}");
        sb.AppendLine($"Aktivační interval (minuty): {kms.ActivationIntervalMinutes}");
        sb.AppendLine($"Lookup doména: {kms.LookupDomain}");
        sb.AppendLine($"Caching povolen: {kms.HostCachingEnabled}");
        sb.AppendLine();
    }

    private static void AppendAdba(StringBuilder sb, AdbaInfo? adba)
    {
        sb.AppendLine("=== ADBA ===");
        if (adba is null)
        {
            sb.AppendLine("ADBA informace nejsou k dispozici.");
            sb.AppendLine();
            return;
        }

        sb.AppendLine($"Object name: {adba.ObjectName}");
        sb.AppendLine($"Object DN: {adba.ObjectDn}");
        sb.AppendLine($"CSVLK PID: {adba.CsvlkPid}");
        sb.AppendLine($"CSVLK SKU: {adba.CsvlkSkuId}");
        sb.AppendLine();
    }

    private static void AppendAvma(StringBuilder sb, AvmaInfo? avma)
    {
        sb.AppendLine("=== AVMA ===");
        if (avma is null)
        {
            sb.AppendLine("AVMA informace nejsou k dispozici.");
            sb.AppendLine();
            return;
        }

        sb.AppendLine($"Hostitel: {avma.HostMachineName}");
        sb.AppendLine($"Hostitel DigitalPID2: {avma.HostDigitalPid2}");
        sb.AppendLine($"Inherited Activation ID: {avma.InheritedActivationId}");
        if (avma.ActivationTime is DateTimeOffset activationTime)
        {
            sb.AppendLine($"Čas aktivace: {activationTime:u}");
        }
        sb.AppendLine();
    }

    private static void AppendSubscription(StringBuilder sb, SubscriptionInfo subscription)
    {
        sb.AppendLine("=== Subscription ===");
        sb.AppendLine($"Podporováno: {subscription.Supported}");
        sb.AppendLine($"Povoleno: {subscription.Enabled}");
        sb.AppendLine($"SKU: {subscription.Sku}");
        sb.AppendLine($"Stav: {subscription.State}");
        sb.AppendLine();
    }

    private static void AppendSkuList(StringBuilder sb, IReadOnlyList<SlSkuEntry> skus)
    {
        sb.AppendLine("=== Nalezené SKU ===");
        if (skus.Count == 0)
        {
            sb.AppendLine("Žádné SKU nebyly nalezeny.");
            return;
        }

        foreach (var sku in skus)
        {
            sb.AppendLine($"- {sku.Name ?? "(bez názvu)"}");
            sb.AppendLine($"  ID: {sku.SkuId}");
            sb.AppendLine($"  Popis: {sku.Description}");
            sb.AppendLine($"  Kanál: {sku.Channel}");
            sb.AppendLine($"  Částečný klíč: {sku.PartialProductKey}");
            sb.AppendLine();
        }
    }
}
