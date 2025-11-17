using System;
using System.Text;
using Nexus.Core.Abstractions;
using Nexus.Core.Models;

namespace Nexus;

/// <summary>
/// Pomocná třída, která orchestruje čtení jednotlivých částí Windows licenčních informací
/// a sestaví z nich přehledný textový report. Slouží jako jednoduchá ukázka využití
/// <see cref="IWindowsInfoFacade"/> bez nutnosti ručního ošetřování výjimek.
/// </summary>
public sealed class NexusDiagnostics
{
    private readonly NexusAPI _api;

    public NexusDiagnostics(IWindowsInfoFacade facade)
    {
        ArgumentNullException.ThrowIfNull(facade);
        _api = new NexusAPI(facade);
    }

    /// <summary>
    /// Provede jednotlivá čtení nad facádou a vrátí textový report.
    /// I v případě selhání dílčích částí se report pokusí dokončit a chybějící
    /// údaje označí.
    /// </summary>
    public string BuildReport()
    {
        var sb = new StringBuilder();

        AppendSystemSection(sb);
        sb.AppendLine();
        AppendKmsSection(sb);
        sb.AppendLine();
        AppendAdbaSection(sb);
        sb.AppendLine();
        AppendAvmaSection(sb);
        sb.AppendLine();
        AppendSubscriptionSection(sb);
        sb.AppendLine();
        AppendSkusSection(sb);

        return sb.ToString();
    }

    private void AppendSystemSection(StringBuilder sb)
    {
        var result = _api.TryGetSystemInfo();
        sb.AppendLine("=== Systém ===");

        if (!result.IsSuccess || result.Data is null)
        {
            sb.AppendLine($"Chyba při načítání: {result.ErrorMessage ?? "neznámá chyba"}.");
            return;
        }

        var info = result.Data;
        sb.AppendLine($"Edice: {info.Edition}");
        sb.AppendLine($"Verze: {info.Version}");
        sb.AppendLine($"Architektura: {info.Architecture}");

        sb.AppendLine("-- Aktivace --");
        var activation = info.Activation;
        sb.AppendLine($"Stav: {activation.Status} ({activation.StatusMessage ?? "bez zprávy"})");
        sb.AppendLine($"Genuine: {activation.Genuine}");
        sb.AppendLine($"Zbývající lhůta (min): {activation.GraceMinutesRemaining?.ToString() ?? "n/a"}");
        sb.AppendLine($"Konec evaluace (UTC): {activation.EvaluationEndUtc?.ToString("u") ?? "n/a"}");
        sb.AppendLine($"Kanál: {activation.Channel ?? "n/a"}");
        sb.AppendLine($"VL Activation Type: {activation.VlActivationType?.ToString() ?? "n/a"}");
        sb.AppendLine($"Partial product key: {activation.PartialProductKey ?? "n/a"}");
        sb.AppendLine($"Extended PID: {activation.ExtendedPid ?? "n/a"}");
        sb.AppendLine($"Product ID: {activation.ProductId ?? "n/a"}");

        sb.AppendLine("-- Produkt --");
        sb.AppendLine($"Typ produktu: {info.Product.ProductTypeCode} ({info.Product.ProductTypeName})");
        sb.AppendLine($"Release: {info.Product.Release}");
    }

    private void AppendKmsSection(StringBuilder sb)
    {
        var result = _api.TryGetKmsClientInfo();
        sb.AppendLine("=== KMS klient ===");

        if (!result.IsSuccess)
        {
            sb.AppendLine($"Chyba při načítání: {result.ErrorMessage ?? "neznámá chyba"}.");
            return;
        }

        if (result.Data is null)
        {
            sb.AppendLine("KMS klient není dostupný nebo je vypnutý.");
            return;
        }

        var info = result.Data;
        sb.AppendLine($"CMID: {info.ClientMachineId ?? "n/a"}");
        sb.AppendLine($"Registrovaný server: {info.RegisteredKmsName ?? "n/a"}:{info.RegisteredKmsPort?.ToString() ?? "n/a"}");
        sb.AppendLine($"Nalezený server: {info.DiscoveredKmsName ?? "n/a"}:{info.DiscoveredKmsPort?.ToString() ?? "n/a"}");
        sb.AppendLine($"Nalezená IP: {info.DiscoveredKmsIp ?? "n/a"}");
        sb.AppendLine($"Aktivační interval (min): {info.ActivationIntervalMinutes?.ToString() ?? "n/a"}");
        sb.AppendLine($"Obnovovací interval (min): {info.RenewalIntervalMinutes?.ToString() ?? "n/a"}");
        sb.AppendLine($"Lookup doména: {info.LookupDomain ?? "n/a"}");
        sb.AppendLine($"Caching hostů: {(info.HostCachingEnabled is null ? "neznámo" : info.HostCachingEnabled.Value ? "zapnuto" : "vypnuto")}");
    }

    private void AppendAdbaSection(StringBuilder sb)
    {
        var result = _api.TryGetAdbaInfo();
        sb.AppendLine("=== ADBA ===");

        if (!result.IsSuccess)
        {
            sb.AppendLine($"Chyba při načítání: {result.ErrorMessage ?? "neznámá chyba"}.");
            return;
        }

        if (result.Data is null)
        {
            sb.AppendLine("ADBA není nakonfigurováno.");
            return;
        }

        var info = result.Data;
        sb.AppendLine($"Object name: {info.ObjectName ?? "n/a"}");
        sb.AppendLine($"Object DN: {info.ObjectDn ?? "n/a"}");
        sb.AppendLine($"CSVLK PID: {info.CsvlkPid ?? "n/a"}");
        sb.AppendLine($"CSVLK SKU ID: {info.CsvlkSkuId ?? "n/a"}");
    }

    private void AppendAvmaSection(StringBuilder sb)
    {
        var result = _api.TryGetAvmaInfo();
        sb.AppendLine("=== AVMA ===");

        if (!result.IsSuccess)
        {
            sb.AppendLine($"Chyba při načítání: {result.ErrorMessage ?? "neznámá chyba"}.");
            return;
        }

        if (result.Data is null)
        {
            sb.AppendLine("AVMA není aktivní.");
            return;
        }

        var info = result.Data;
        sb.AppendLine($"Host: {info.HostMachineName ?? "n/a"}");
        sb.AppendLine($"Host DigitalPID2: {info.HostDigitalPid2 ?? "n/a"}");
        sb.AppendLine($"Inherited Activation ID: {info.InheritedActivationId ?? "n/a"}");
        sb.AppendLine($"Čas aktivace: {info.ActivationTime?.ToString("u") ?? "n/a"}");
    }

    private void AppendSubscriptionSection(StringBuilder sb)
    {
        var result = _api.TryGetSubscriptionInfo();
        sb.AppendLine("=== Předplatné ===");

        if (!result.IsSuccess || result.Data is null)
        {
            sb.AppendLine($"Chyba při načítání: {result.ErrorMessage ?? "neznámá chyba"}.");
            return;
        }

        var info = result.Data;
        sb.AppendLine($"Podporováno: {(info.Supported ? "ano" : "ne")}");
        sb.AppendLine($"Povoleno: {(info.Enabled ? "ano" : "ne")}");
        sb.AppendLine($"SKU: {info.Sku?.ToString() ?? "n/a"}");
        sb.AppendLine($"Stav: {info.State?.ToString() ?? "n/a"}");
    }

    private void AppendSkusSection(StringBuilder sb)
    {
        var result = _api.TryGetSkuEntries();
        sb.AppendLine("=== SKU ===");

        if (!result.IsSuccess || result.Data is null)
        {
            sb.AppendLine($"Chyba při načítání: {result.ErrorMessage ?? "neznámá chyba"}.");
            return;
        }

        if (result.Data.Count == 0)
        {
            sb.AppendLine("Nebyly nalezeny žádné záznamy.");
            return;
        }

        foreach (var entry in result.Data)
        {
            sb.AppendLine($"- {entry.Name ?? "Bez názvu"} ({entry.SkuId})");
            sb.AppendLine($"  Popis: {entry.Description ?? "n/a"}");
            sb.AppendLine($"  Kanál: {entry.Channel ?? "n/a"}");
            sb.AppendLine($"  Partial key: {entry.PartialProductKey ?? "n/a"}");
        }
    }
}
