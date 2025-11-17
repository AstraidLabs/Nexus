using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Nexus.Core.Abstractions;
using Nexus.Core.Enums;
using Nexus.Core.Models;

namespace Nexus.ConsoleTest;

/// <summary>
/// Náhradní implementace pro prostředí mimo Windows, která dodá ukázková data
/// místo přímého čtení systémových API.
/// </summary>
internal sealed class NonWindowsInfoFacade : IWindowsInfoFacade
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
        Sku = "N/A",
        State = "N/A"
    };

    public IReadOnlyList<SlSkuEntry> GetSkus() => Array.Empty<SlSkuEntry>();
}
