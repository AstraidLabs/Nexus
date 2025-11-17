using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Nexus.Core.Abstractions;
using Nexus.Core.Enums;
using Nexus.Core.Models;
using Nexus.Core.Services;

namespace Nexus;

/// <summary>
/// Vstupní bod knihovny, který sjednocuje běžné operace nad <see cref="IWindowsInfoFacade"/>.
/// Poskytuje metody s ošetřením výjimek i varianty bez vyvolání výjimek, aby byla integrace
/// do dalších projektů co nejjednodušší.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class NexusAPI
{
    private readonly IWindowsInfoFacade _facade;

    /// <summary>
    /// Vytvoří novou instanci API. Pokud není dodána vlastní implementace <see cref="IWindowsInfoFacade"/>,
    /// použije se výchozí implementace pro aktuální platformu.
    /// </summary>
    /// <param name="facade">Volitelná vlastní implementace.</param>
    /// <exception cref="ArgumentNullException">Pokud je předána hodnota <c>null</c>.</exception>
    public NexusAPI(IWindowsInfoFacade? facade = null)
    {
        _facade = facade ?? CreateDefaultFacade();
    }

    /// <summary>
    /// Načte informace o systému. Případná selhání jsou převedena na <see cref="NexusApiException"/>.
    /// </summary>
    public WindowsSystemData GetSystemInfo() => Execute(_facade.GetSystem, "Nepodařilo se načíst informace o systému.");

    /// <summary>
    /// Varianty bez vyvolání výjimky pro získání informací o systému.
    /// </summary>
    public NexusOperationResult<WindowsSystemData> TryGetSystemInfo() => TryExecute(_facade.GetSystem, "Nepodařilo se načíst informace o systému.");

    public KmsClientInfo? GetKmsClientInfo() => Execute(_facade.GetKmsClient, "Nepodařilo se načíst informace o KMS klientovi.");
    public NexusOperationResult<KmsClientInfo?> TryGetKmsClientInfo() => TryExecute(_facade.GetKmsClient, "Nepodařilo se načíst informace o KMS klientovi.");

    public AdbaInfo? GetAdbaInfo() => Execute(_facade.GetAdba, "Nepodařilo se načíst informace o ADBA.");
    public NexusOperationResult<AdbaInfo?> TryGetAdbaInfo() => TryExecute(_facade.GetAdba, "Nepodařilo se načíst informace o ADBA.");

    public AvmaInfo? GetAvmaInfo() => Execute(_facade.GetAvma, "Nepodařilo se načíst informace o AVMA.");
    public NexusOperationResult<AvmaInfo?> TryGetAvmaInfo() => TryExecute(_facade.GetAvma, "Nepodařilo se načíst informace o AVMA.");

    public SubscriptionInfo GetSubscriptionInfo() => Execute(_facade.GetSubscription, "Nepodařilo se načíst informace o předplatném.");
    public NexusOperationResult<SubscriptionInfo> TryGetSubscriptionInfo() => TryExecute(_facade.GetSubscription, "Nepodařilo se načíst informace o předplatném.");

    public IReadOnlyList<SlSkuEntry> GetSkuEntries() => Execute(_facade.GetSkus, "Nepodařilo se načíst seznam SKU.");
    public NexusOperationResult<IReadOnlyList<SlSkuEntry>> TryGetSkuEntries() => TryExecute(_facade.GetSkus, "Nepodařilo se načíst seznam SKU.");

    private static T Execute<T>(Func<T> action, string context)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            throw new NexusApiException(context, ex);
        }
    }

    private static NexusOperationResult<T> TryExecute<T>(Func<T> action, string context)
    {
        try
        {
            return NexusOperationResult<T>.Success(action());
        }
        catch (Exception ex)
        {
            return NexusOperationResult<T>.Failure(context, ex);
        }
    }

    private static IWindowsInfoFacade CreateDefaultFacade()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsInfoFacade();
        }

        return new SampleFacade();
    }

    /// <summary>
    /// Fallback pro ne-Windows prostředí, aby bylo možné API využívat i v testech či CI bez
    /// přímého přístupu k nativním Windows API.
    /// </summary>
    private sealed class SampleFacade : IWindowsInfoFacade
    {
        public WindowsSystemData GetSystem() => new()
        {
            Edition = "N/A",
            Version = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            Activation = new ActivationInfo
            {
                Status = LicensingStatus.Unknown,
                StatusMessage = "Aktivační údaje jsou dostupné pouze na Windows.",
                Genuine = GenuineState.Offline,
                Channel = "N/A",
                PartialProductKey = null,
                ExtendedPid = null,
                ProductId = null
            },
            Product = new ProductInfo
            {
                ProductTypeCode = 0,
                ProductTypeName = "Neznámý systém",
                Release = "N/A"
            }
        };

        public KmsClientInfo? GetKmsClient() => null;

        public AdbaInfo? GetAdba() => null;

        public AvmaInfo? GetAvma() => null;

        public SubscriptionInfo GetSubscription() => new()
        {
            Supported = false,
            Enabled = false,
            Sku = null,
            State = null
        };

        public IReadOnlyList<SlSkuEntry> GetSkus() => Array.Empty<SlSkuEntry>();
    }
}

/// <summary>
/// Standardizovaný výsledek pro volání NexusAPI, který nikdy nevyhazuje výjimku.
/// </summary>
/// <typeparam name="T">Typ vrácené hodnoty.</typeparam>
public sealed class NexusOperationResult<T>
{
    private NexusOperationResult(T? data, string? errorMessage, Exception? exception)
    {
        Data = data;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public T? Data { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    public bool IsSuccess => Exception is null;

    public static NexusOperationResult<T> Success(T data) => new(data, null, null);

    public static NexusOperationResult<T> Failure(string errorMessage, Exception exception) => new(default, errorMessage, exception);
}
