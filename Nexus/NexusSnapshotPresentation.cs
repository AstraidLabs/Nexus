using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nexus.Core.Models;

namespace Nexus;

/// <summary>
/// Pomocný mapper, který převádí snapshot do struktury vhodné pro konzolové i XAML aplikace.
/// </summary>
public sealed class NexusSnapshotPresenter
{
    public NexusPresentationModel Prepare(NexusSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var sections = new List<NexusPresentationSection>
        {
            CreateSection("Systém", snapshot.System, BuildSystemFields),
            CreateSection("KMS klient", snapshot.Kms, BuildKmsFields),
            CreateSection("ADBA", snapshot.Adba, BuildAdbaFields),
            CreateSection("AVMA", snapshot.Avma, BuildAvmaFields),
            CreateSection("Předplatné", snapshot.Subscription, BuildSubscriptionFields),
            CreateSection("SKU", snapshot.Skus, BuildSkuFields)
        };

        return new NexusPresentationModel(sections, snapshot.Logs);
    }

    private static NexusPresentationSection CreateSection<T>(
        string title,
        NexusSnapshotSection<T> section,
        Func<T, IEnumerable<NexusPresentationField>> fieldFactory)
    {
        if (!section.IsSuccess)
        {
            return new NexusPresentationSection(title, NexusPresentationState.Error, section.ErrorMessage ?? "Neznámá chyba.", Array.Empty<NexusPresentationField>());
        }

        if (section.Data is null)
        {
            return new NexusPresentationSection(title, NexusPresentationState.Warning, "Data nejsou k dispozici.", Array.Empty<NexusPresentationField>());
        }

        var fields = fieldFactory(section.Data).ToArray();
        return new NexusPresentationSection(title, NexusPresentationState.Ok, null, fields);
    }

    private static IEnumerable<NexusPresentationField> BuildSystemFields(WindowsSystemData data)
    {
        yield return new NexusPresentationField("Edice", data.Edition);
        yield return new NexusPresentationField("Verze", data.Version);
        yield return new NexusPresentationField("Architektura", data.Architecture);
        yield return new NexusPresentationField("Stav aktivace", $"{data.Activation.Status} ({data.Activation.StatusMessage ?? "bez zprávy"})");
        yield return new NexusPresentationField("Kanál", data.Activation.Channel ?? "n/a");
        yield return new NexusPresentationField("Release", data.Product.Release);
    }

    private static IEnumerable<NexusPresentationField> BuildKmsFields(KmsClientInfo? data)
    {
        if (data is null)
        {
            yield break;
        }

        yield return new NexusPresentationField("CMID", data.ClientMachineId ?? "n/a");
        yield return new NexusPresentationField("Registrovaný server", $"{data.RegisteredKmsName ?? "n/a"}:{data.RegisteredKmsPort?.ToString() ?? "n/a"}");
        yield return new NexusPresentationField("Nalezený server", $"{data.DiscoveredKmsName ?? "n/a"}:{data.DiscoveredKmsPort?.ToString() ?? "n/a"}");
        yield return new NexusPresentationField("Nalezená IP", data.DiscoveredKmsIp ?? "n/a");
        yield return new NexusPresentationField("Aktivační interval", data.ActivationIntervalMinutes?.ToString() ?? "n/a");
        yield return new NexusPresentationField("Obnovovací interval", data.RenewalIntervalMinutes?.ToString() ?? "n/a");
        yield return new NexusPresentationField("Lookup doména", data.LookupDomain ?? "n/a");
        yield return new NexusPresentationField("Caching hostů", data.HostCachingEnabled switch
        {
            true => "zapnuto",
            false => "vypnuto",
            _ => "neznámo"
        });
    }

    private static IEnumerable<NexusPresentationField> BuildAdbaFields(AdbaInfo? data)
    {
        if (data is null)
        {
            yield break;
        }

        yield return new NexusPresentationField("Object name", data.ObjectName ?? "n/a");
        yield return new NexusPresentationField("Object DN", data.ObjectDn ?? "n/a");
        yield return new NexusPresentationField("CSVLK PID", data.CsvlkPid ?? "n/a");
        yield return new NexusPresentationField("CSVLK SKU ID", data.CsvlkSkuId ?? "n/a");
    }

    private static IEnumerable<NexusPresentationField> BuildAvmaFields(AvmaInfo? data)
    {
        if (data is null)
        {
            yield break;
        }

        yield return new NexusPresentationField("Host", data.HostMachineName ?? "n/a");
        yield return new NexusPresentationField("Host DigitalPID2", data.HostDigitalPid2 ?? "n/a");
        yield return new NexusPresentationField("Inherited Activation ID", data.InheritedActivationId ?? "n/a");
        yield return new NexusPresentationField("Čas aktivace", data.ActivationTime?.ToString("u") ?? "n/a");
    }

    private static IEnumerable<NexusPresentationField> BuildSubscriptionFields(SubscriptionInfo data)
    {
        yield return new NexusPresentationField("Podporováno", data.Supported ? "ano" : "ne");
        yield return new NexusPresentationField("Povoleno", data.Enabled ? "ano" : "ne");
        yield return new NexusPresentationField("SKU", data.Sku?.ToString() ?? "n/a");
        yield return new NexusPresentationField("Stav", data.State?.ToString() ?? "n/a");
    }

    private static IEnumerable<NexusPresentationField> BuildSkuFields(IReadOnlyList<SlSkuEntry> entries)
    {
        if (entries.Count == 0)
        {
            yield return new NexusPresentationField("Záznamy", "Žádné položky nebyly nalezeny.");
            yield break;
        }

        foreach (var entry in entries)
        {
            yield return new NexusPresentationField(entry.Name ?? "Bez názvu", entry.SkuId.ToString());
        }
    }
}

/// <summary>
/// Výsledek převodu snapshotu do prezentační vrstvy.
/// </summary>
public sealed class NexusPresentationModel
{
    public NexusPresentationModel(IReadOnlyList<NexusPresentationSection> sections, IReadOnlyCollection<NexusLogEntry> logs)
    {
        Sections = sections;
        Logs = logs;
    }

    public IReadOnlyList<NexusPresentationSection> Sections { get; }
    public IReadOnlyCollection<NexusLogEntry> Logs { get; }

    public bool HasErrors => Sections.Any(section => section.State == NexusPresentationState.Error);

    /// <summary>
    /// Jednoduchý textový výstup pro konzoli nebo log.
    /// </summary>
    public string ToConsoleString()
    {
        var sb = new StringBuilder();

        foreach (var section in Sections)
        {
            sb.AppendLine($"=== {section.Title} ===");

            if (section.Fields.Count == 0)
            {
                sb.AppendLine(section.ErrorMessage ?? "Žádná data nejsou k dispozici.");
                sb.AppendLine();
                continue;
            }

            foreach (var field in section.Fields)
            {
                sb.AppendLine($"- {field.Label}: {field.Value ?? "n/a"}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}

public sealed record NexusPresentationSection(string Title, NexusPresentationState State, string? ErrorMessage, IReadOnlyList<NexusPresentationField> Fields);

public sealed record NexusPresentationField(string Label, string? Value);

public enum NexusPresentationState
{
    Ok,
    Warning,
    Error
}
