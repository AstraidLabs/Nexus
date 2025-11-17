using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus;
using Nexus.Core.Abstractions;
using Nexus.Core.Enums;
using Nexus.Core.Models;

namespace Nexus.Tests;

[TestClass]
public class NexusClientTests
{
    [TestMethod]
    public void CaptureSnapshot_SuccessfulOperations_LogEntriesCreated()
    {
        var facade = new ConfigurableFacade();
        var sinkEntries = new List<NexusLogEntry>();

        var client = NexusClient.Create(facade, options => options.LogSink = sinkEntries.Add);

        var snapshot = client.CaptureSnapshot();

        Assert.IsFalse(snapshot.HasErrors);
        Assert.AreEqual("Test Edition", snapshot.System.Data!.Edition);
        Assert.AreEqual("TestVersion", snapshot.System.Data!.Version);
        Assert.AreEqual("x64", snapshot.System.Data!.Architecture);
        Assert.AreEqual(LicensingStatus.Licensed, snapshot.System.Data!.Activation.Status);
        Assert.AreEqual("Scope", snapshot.Kms.Data!.RegisteredKmsName);
        Assert.IsTrue(snapshot.Subscription.Data!.Supported);
        Assert.AreEqual(2, snapshot.Skus.Data!.Count);

        Assert.AreEqual(6, snapshot.Logs.Count);
        Assert.AreEqual(snapshot.Logs.Count, sinkEntries.Count);
        Assert.IsTrue(snapshot.Logs.All(entry => entry.Level == NexusLogLevel.Information));
        CollectionAssert.AreEqual(snapshot.Logs.ToList(), sinkEntries.ToList());
    }

    [TestMethod]
    public void CaptureSnapshot_LogsWarningsWhenFacadeThrows()
    {
        var facade = new ConfigurableFacade(
            kms: () => throw new InvalidOperationException("KMS selhalo"));

        var client = NexusClient.Create(facade);

        var snapshot = client.CaptureSnapshot();

        Assert.IsTrue(snapshot.HasErrors);
        Assert.IsFalse(snapshot.Kms.IsSuccess);
        Assert.AreEqual("Nepodařilo se načíst informace o KMS klientovi.", snapshot.Kms.ErrorMessage);

        var warning = snapshot.Logs.Single(entry => entry.Scope == "kms");
        Assert.AreEqual(NexusLogLevel.Warning, warning.Level);
        StringAssert.Contains(warning.Message, "Nepodařilo se načíst informace o KMS klientovi.");
        Assert.IsNotNull(warning.Exception);
    }
}

[TestClass]
public class NexusDiagnosticsTests
{
    [TestMethod]
    public void BuildReport_IncludesValuesAndFallbackMessages()
    {
        var facade = new ConfigurableFacade(
            kms: () => null,
            subscription: () => throw new InvalidOperationException("sub failure"));

        var diagnostics = new NexusDiagnostics(facade);

        var report = diagnostics.BuildReport();

        StringAssert.Contains(report, "=== Systém ===");
        StringAssert.Contains(report, "Edice: Test Edition");
        StringAssert.Contains(report, "Genuine: Genuine");

        StringAssert.Contains(report, "=== KMS klient ===");
        StringAssert.Contains(report, "KMS klient není dostupný nebo je vypnutý.");

        StringAssert.Contains(report, "=== Předplatné ===");
        StringAssert.Contains(report, "Chyba při načítání: Nepodařilo se načíst informace o předplatném.");
    }
}

internal sealed class ConfigurableFacade : IWindowsInfoFacade
{
    private readonly Func<WindowsSystemData> _system;
    private readonly Func<KmsClientInfo?> _kms;
    private readonly Func<AdbaInfo?> _adba;
    private readonly Func<AvmaInfo?> _avma;
    private readonly Func<SubscriptionInfo> _subscription;
    private readonly Func<IReadOnlyList<SlSkuEntry>> _skus;

    public ConfigurableFacade(
        Func<WindowsSystemData>? system = null,
        Func<KmsClientInfo?>? kms = null,
        Func<AdbaInfo?>? adba = null,
        Func<AvmaInfo?>? avma = null,
        Func<SubscriptionInfo>? subscription = null,
        Func<IReadOnlyList<SlSkuEntry>>? skus = null)
    {
        _system = system ?? CreateDefaultSystem;
        _kms = kms ?? CreateDefaultKms;
        _adba = adba ?? CreateDefaultAdba;
        _avma = avma ?? CreateDefaultAvma;
        _subscription = subscription ?? CreateDefaultSubscription;
        _skus = skus ?? CreateDefaultSkus;
    }

    public WindowsSystemData GetSystem() => _system();

    public KmsClientInfo? GetKmsClient() => _kms();

    public AdbaInfo? GetAdba() => _adba();

    public AvmaInfo? GetAvma() => _avma();

    public SubscriptionInfo GetSubscription() => _subscription();

    public IReadOnlyList<SlSkuEntry> GetSkus() => _skus();

    private static WindowsSystemData CreateDefaultSystem() => new()
    {
        Edition = "Test Edition",
        Version = "TestVersion",
        Architecture = "x64",
        Activation = new ActivationInfo
        {
            Status = LicensingStatus.Licensed,
            StatusMessage = "OK",
            Genuine = GenuineState.Genuine,
            GraceMinutesRemaining = 120,
            EvaluationEndUtc = DateTimeOffset.UnixEpoch,
            Channel = "Retail",
            VlActivationType = VolumeActivationType.ActiveDirectory,
            PartialProductKey = "12345",
            ExtendedPid = "ext",
            ProductId = "pid"
        },
        Product = new ProductInfo
        {
            ProductTypeCode = 1,
            ProductTypeName = "Test Product",
            Release = "1.0"
        }
    };

    private static KmsClientInfo CreateDefaultKms() => new()
    {
        ClientMachineId = "client",
        RegisteredKmsName = "Scope",
        RegisteredKmsPort = 1688,
        DiscoveredKmsName = "kms.local",
        DiscoveredKmsPort = 1689,
        DiscoveredKmsIp = "127.0.0.1",
        ActivationIntervalMinutes = 15,
        RenewalIntervalMinutes = 30,
        LookupDomain = "example.com",
        HostCachingEnabled = true
    };

    private static AdbaInfo CreateDefaultAdba() => new()
    {
        ObjectName = "ADBA",
        ObjectDn = "CN=ADBA",
        CsvlkPid = "pid",
        CsvlkSkuId = "sku"
    };

    private static AvmaInfo CreateDefaultAvma() => new()
    {
        HostMachineName = "host",
        HostDigitalPid2 = "pid2",
        InheritedActivationId = "activation",
        ActivationTime = DateTimeOffset.UnixEpoch
    };

    private static SubscriptionInfo CreateDefaultSubscription() => new()
    {
        Supported = true,
        Enabled = true,
        Sku = 1,
        State = 2
    };

    private static IReadOnlyList<SlSkuEntry> CreateDefaultSkus() => new[]
    {
        new SlSkuEntry { SkuId = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "SKU1", Description = "Desc1", Channel = "Retail", PartialProductKey = "ABCDE" },
        new SlSkuEntry { SkuId = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "SKU2", Description = "Desc2", Channel = "OEM", PartialProductKey = "FGHIJ" }
    };
}
