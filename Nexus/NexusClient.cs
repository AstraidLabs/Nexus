using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.Core.Abstractions;
using Nexus.Core.Models;

namespace Nexus;

/// <summary>
/// Moderní vstupní bod, který sjednocuje načítání dat, logování a předpřipravenou prezentaci
/// pro konzolové i desktopové aplikace. Knihovna tak může být okamžitě použitá bez nutnosti
/// psát opakující se kód pro validaci nebo budování UI modelu.
/// </summary>
public sealed class NexusClient
{
    private readonly NexusAPI _api;
    private readonly NexusClientOptions _options;

    private NexusClient(NexusAPI api, NexusClientOptions options)
    {
        _api = api;
        _options = options;
    }

    /// <summary>
    /// Vytvoří nového klienta. Pokud není předána vlastní facáda, použije se výchozí.
    /// </summary>
    public static NexusClient Create(IWindowsInfoFacade? facade = null, Action<NexusClientOptions>? configure = null)
    {
        var options = NexusClientOptions.Create(configure);
        return new NexusClient(new NexusAPI(facade), options);
    }

    /// <summary>
    /// Provede jedno hromadné načtení všech dostupných dat a vrátí strukturovaný snapshot.
    /// Všechny chyby jsou zachyceny do logu a zároveň k dispozici v jednotlivých sekcích.
    /// </summary>
    public NexusSnapshot CaptureSnapshot()
    {
        var logs = new List<NexusLogEntry>();

        var system = CaptureSection("system", _api.TryGetSystemInfo, logs);
        var kms = CaptureSection("kms", _api.TryGetKmsClientInfo, logs);
        var adba = CaptureSection("adba", _api.TryGetAdbaInfo, logs);
        var avma = CaptureSection("avma", _api.TryGetAvmaInfo, logs);
        var subscription = CaptureSection("subscription", _api.TryGetSubscriptionInfo, logs);
        var skus = CaptureSection("sku", _api.TryGetSkuEntries, logs);

        return new NexusSnapshot(system, kms, adba, avma, subscription, skus, logs);
    }

    private NexusSnapshotSection<T> CaptureSection<T>(
        string name,
        Func<NexusOperationResult<T>> operation,
        ICollection<NexusLogEntry> logs)
    {
        ArgumentNullException.ThrowIfNull(operation);

        try
        {
            var result = operation();

            if (result.IsSuccess)
            {
                var entry = NexusLogEntry.Information(name, "Načteno úspěšně.");
                AppendLog(entry, logs);
                return NexusSnapshotSection<T>.Success(name, result.Data);
            }

            var errorEntry = NexusLogEntry.Warning(name, result.ErrorMessage ?? "Neznámá chyba.", result.Exception);
            AppendLog(errorEntry, logs);
            return NexusSnapshotSection<T>.Failure(name, result.ErrorMessage ?? "Neznámá chyba.");
        }
        catch (Exception ex)
        {
            var entry = NexusLogEntry.Error(name, "Neočekávaná chyba při načítání.", ex);
            AppendLog(entry, logs);
            return NexusSnapshotSection<T>.Failure(name, ex.Message);
        }
    }

    private void AppendLog(NexusLogEntry entry, ICollection<NexusLogEntry> logs)
    {
        logs.Add(entry);
        _options.LogSink?.Invoke(entry);
    }
}

/// <summary>
/// Nastavení klienta, které umožní předat externí log sink.
/// </summary>
public sealed class NexusClientOptions
{
    public Action<NexusLogEntry>? LogSink { get; set; }

    internal static NexusClientOptions Create(Action<NexusClientOptions>? configure)
    {
        var options = new NexusClientOptions();
        configure?.Invoke(options);
        return options;
    }
}

/// <summary>
/// Zachycené informace z jedné sekce čtení.
/// </summary>
public sealed class NexusSnapshotSection<T> : INexusSnapshotSection
{
    private NexusSnapshotSection(string name, T? data, string? errorMessage, bool isSuccess)
    {
        Name = name;
        Data = data;
        ErrorMessage = errorMessage;
        IsSuccess = isSuccess;
    }

    public string Name { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess { get; }

    object? INexusSnapshotSection.BoxedData => Data;

    public static NexusSnapshotSection<T> Success(string name, T? data) => new(name, data, null, true);
    public static NexusSnapshotSection<T> Failure(string name, string? errorMessage) => new(name, default, errorMessage, false);
}

/// <summary>
/// Společný pohled na všechny sekce snapshotu pro snadnou prezentaci v UI.
/// </summary>
public interface INexusSnapshotSection
{
    string Name { get; }
    string? ErrorMessage { get; }
    bool IsSuccess { get; }
    object? BoxedData { get; }
}

/// <summary>
/// Kompletní stav z jednoho načtení všech dat.
/// </summary>
public sealed class NexusSnapshot
{
    public NexusSnapshot(
        NexusSnapshotSection<WindowsSystemData> system,
        NexusSnapshotSection<KmsClientInfo?> kms,
        NexusSnapshotSection<AdbaInfo?> adba,
        NexusSnapshotSection<AvmaInfo?> avma,
        NexusSnapshotSection<SubscriptionInfo> subscription,
        NexusSnapshotSection<IReadOnlyList<SlSkuEntry>> skus,
        IReadOnlyCollection<NexusLogEntry> logs)
    {
        System = system;
        Kms = kms;
        Adba = adba;
        Avma = avma;
        Subscription = subscription;
        Skus = skus;
        Logs = logs;

        Sections = new INexusSnapshotSection[] { system, kms, adba, avma, subscription, skus };
    }

    public NexusSnapshotSection<WindowsSystemData> System { get; }
    public NexusSnapshotSection<KmsClientInfo?> Kms { get; }
    public NexusSnapshotSection<AdbaInfo?> Adba { get; }
    public NexusSnapshotSection<AvmaInfo?> Avma { get; }
    public NexusSnapshotSection<SubscriptionInfo> Subscription { get; }
    public NexusSnapshotSection<IReadOnlyList<SlSkuEntry>> Skus { get; }

    public IReadOnlyCollection<NexusLogEntry> Logs { get; }
    public IReadOnlyCollection<INexusSnapshotSection> Sections { get; }

    public bool HasErrors => Sections.Any(section => !section.IsSuccess);
}

/// <summary>
/// Jeden záznam interního logu. Umožňuje ho předat do vlastního loggeru nebo zobrazit uživateli.
/// </summary>
public sealed record NexusLogEntry(DateTimeOffset Timestamp, NexusLogLevel Level, string Scope, string Message, Exception? Exception)
{
    public static NexusLogEntry Information(string scope, string message) => new(DateTimeOffset.UtcNow, NexusLogLevel.Information, scope, message, null);

    public static NexusLogEntry Warning(string scope, string message, Exception? exception = null) => new(DateTimeOffset.UtcNow, NexusLogLevel.Warning, scope, message, exception);

    public static NexusLogEntry Error(string scope, string message, Exception exception) => new(DateTimeOffset.UtcNow, NexusLogLevel.Error, scope, message, exception);
}

/// <summary>
/// Úrovně interního logu.
/// </summary>
public enum NexusLogLevel
{
    Trace = 0,
    Information,
    Warning,
    Error
}
