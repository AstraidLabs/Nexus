using Nexus.Core.Abstractions;
using Nexus.Core.Constants;
using Nexus.Core.Enums;
using Nexus.Core.Interop;
using Nexus.Core.Models;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Nexus.Core.Services;

[SupportedOSPlatform("windows")]
public sealed class WindowsSystemReader : IWindowsSystemReader
{
    public WindowsSystemData GetSystemData()
    {
        var product = GetProductInfo();
        var act = GetActivationInfo();
        return new WindowsSystemData
        {
            Edition = product.ProductTypeName,
            Version = Environment.OSVersion.Version.ToString(),
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            Activation = act,
            Product = product
        };
    }

    public ProductInfo GetProductInfo()
    {
        uint code = 0;
        var v = Environment.OSVersion.Version;

        if (Kernel32.GetProductInfo(v.Major, v.Minor, 0, 0, out var prodType))
            code = prodType;

        ProductTypeMap.Names.TryGetValue(code, out var name);
        name ??= "Unknown";

        var k32 = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\kernel32.dll");
        var release = FileVersionInfo.GetVersionInfo(k32).FileVersion ?? "";

        return new ProductInfo
        {
            ProductTypeCode = code,
            ProductTypeName = name,
            Release = release
        };
    }

    public ActivationInfo GetActivationInfo()
    {
        IntPtr h = IntPtr.Zero;
        try
        {
            if (Slapi.SLOpen(out h) != 0 || h == IntPtr.Zero)
                return Unknown();

            var app = new Guid(WellKnown.WindowsAppId);

            // Získej list SKU ID pro Windows
            uint count = 0;
            IntPtr idsPtr = IntPtr.Zero;
            var hr = Slapi.SLGetSLIDList(h, 0, ref app, 1, ref count, out idsPtr);
            if (hr != 0 || count == 0 || idsPtr == IntPtr.Zero)
                return Unknown();

            // Pole GUIDů (16 bajtů na GUID)
            Guid? pickedSku = null;
            for (int i = 0; i < count; i++)
            {
                var skuPtr = idsPtr + (i * 16);
                var skuId = Marshal.PtrToStructure<Guid>(skuPtr);

                // Zkusíme načíst status pro dané SKU
                var info = ReadStatusForSku(h, app, skuId);
                if (info is { Status: LicensingStatus.Licensed })
                {
                    pickedSku = skuId;
                    break;
                }

                // fallback: první nenulový
                pickedSku ??= skuId;
            }

            // bezpečnost: uvolnit návrat z SLGetSLIDList (alokováno API)
            SlApiBuffer.TryFree(idsPtr);

            if (pickedSku is null)
                return Unknown();

            // Finální načtení pro vybraný SKU
            return ReadFullActivationInfo(h, app, pickedSku.Value);
        }
        finally
        {
            if (h != IntPtr.Zero) _ = Slapi.SLClose(h);
        }

        static ActivationInfo Unknown() => new() { Status = LicensingStatus.Unknown };
    }

    public PKeyInfo? GetInstalledProductKeyPartial()
    {
        var act = GetActivationInfo();
        if (string.IsNullOrWhiteSpace(act.PartialProductKey) && string.IsNullOrWhiteSpace(act.Channel))
            return null;

        return new PKeyInfo
        {
            PartialProductKey = act.PartialProductKey,
            Channel = act.Channel,
            DigitalPid = null
        };
    }

    // --- Interní pomocníci ---

    private static ActivationInfo? ReadStatusForSku(IntPtr h, Guid app, Guid sku)
    {
        // Minimální čtení pro výběr kandidáta (status + grace)
        if (Slapi.SLGetLicensingStatusInformation(h, ref app, ref sku, IntPtr.Zero, ref UnsafeUInt0, out var pStatus) != 0 || UnsafeUInt0 == 0 || pStatus == IntPtr.Zero)
            return null;

        try
        {
            var rawStatus = Marshal.ReadInt32(pStatus, 16); // dwStatus
            var status = NormalizeStatus(rawStatus);
            return new ActivationInfo { Status = status };
        }
        finally
        {
            SlApiBuffer.TryFree(pStatus);
        }
    }

    private static ActivationInfo ReadFullActivationInfo(IntPtr h, Guid app, Guid sku)
    {
        // 1) LicensingStatusInformation
        uint cStatus = 0;
        IntPtr pStatus = IntPtr.Zero;
        var hr = Slapi.SLGetLicensingStatusInformation(h, ref app, ref sku, IntPtr.Zero, ref cStatus, out pStatus);
        if (hr != 0 || cStatus == 0 || pStatus == IntPtr.Zero)
            return new ActivationInfo { Status = LicensingStatus.Unknown };

        int rawStatus;
        int grace;
        int reason;
        long validity;

        try
        {
            rawStatus = Marshal.ReadInt32(pStatus, 16);  // dwStatus
            grace = Marshal.ReadInt32(pStatus, 20);  // dwGrace
            reason = Marshal.ReadInt32(pStatus, 28);  // hrReason
            validity = Marshal.ReadInt64(pStatus, 32);  // qwValidity (FILETIME)
        }
        finally
        {
            SlApiBuffer.TryFree(pStatus);
        }

        var status = NormalizeStatus(rawStatus);

        // 2) Genuine
        uint genuine = 0;
        var ghr = Slapi.SLIsWindowsGenuineLocal(out genuine);
        var gState = (ghr == 0 && genuine <= 4) ? (GenuineState)genuine : GenuineState.Last;

        // 3) Channel + Partial Key (stringy je nutné po přečtení uvolnit)
        string? channel = TryReadStringSku(h, sku, "Channel");
        string? ppk = TryReadStringSku(h, sku, "PartialProductKey");

        int? vlType = TryReadIntSku(h, sku, "VLActivationType");
        Guid? pkeyId = TryReadGuidSku(h, sku, "pkeyId");

        string? extendedPid = null;
        string? productId = null;

        if (pkeyId is Guid pk)
        {
            extendedPid = TryReadStringPkey(h, pk, "DigitalPID");
            productId = TryReadStringPkey(h, pk, "DigitalPID2");
            channel ??= TryReadStringPkey(h, pk, "Channel");
            ppk ??= TryReadStringPkey(h, pk, "PartialProductKey");
        }

        return new ActivationInfo
        {
            Status = status,
            StatusMessage = reason != 0 ? $"0x{reason:X8}" : null,
            Genuine = gState,
            GraceMinutesRemaining = grace > 0 ? grace : null,
            EvaluationEndUtc = validity > 0 ? DateTimeOffset.FromFileTime(validity) : null,
            Channel = channel,
            PartialProductKey = ppk,
            VlActivationType = vlType is int rawVl && Enum.IsDefined(typeof(VolumeActivationType), rawVl)
                ? (VolumeActivationType)rawVl
                : null,
            ExtendedPid = extendedPid,
            ProductId = productId
        };
    }

    private static LicensingStatus NormalizeStatus(int raw)
    {
        // Přibližná normalizace podle skriptu (raw 3 => Notification, atd.)
        return raw switch
        {
            0 => LicensingStatus.Unlicensed,
            1 => LicensingStatus.Licensed,
            2 => LicensingStatus.InGracePeriod,
            3 => LicensingStatus.Notification, // „Additional grace“ v PS je mapováno na 5→3 atd.
            4 => LicensingStatus.InGracePeriod, // Non-genuine grace (v praxi zachytíš důvod v StatusMessage)
            5 => LicensingStatus.Notification,
            _ => LicensingStatus.Unknown
        };
    }

    private static string? TryReadStringSku(IntPtr h, Guid sku, string name)
    {
        if (Slapi.SLGetProductSkuInformation(h, ref sku, name, out var type, out var cb, out var ptr) != 0 || cb == 0 || type != 1 || ptr == IntPtr.Zero)
            return null;

        return SlApiBuffer.ReadAndFreeUniString(ptr);
    }

    private static string? TryReadStringPkey(IntPtr h, Guid id, string name)
    {
        if (Slapi.SLGetPKeyInformation(h, ref id, name, out var type, out var cb, out var ptr) != 0 || cb == 0 || type != 1 || ptr == IntPtr.Zero)
            return null;

        return SlApiBuffer.ReadAndFreeUniString(ptr);
    }

    private static int? TryReadIntSku(IntPtr h, Guid sku, string name)
    {
        if (Slapi.SLGetProductSkuInformation(h, ref sku, name, out var type, out var cb, out var ptr) != 0 || cb == 0 || type != 4 || ptr == IntPtr.Zero)
            return null;

        try { return Marshal.ReadInt32(ptr); }
        finally { SlApiBuffer.TryFree(ptr); }
    }

    private static Guid? TryReadGuidSku(IntPtr h, Guid sku, string name)
    {
        var raw = TryReadStringSku(h, sku, name);
        return Guid.TryParse(raw, out var guid) ? guid : null;
    }

    // malý hack: SLGetLicensingStatusInformation vyžaduje ref param pro count; držíme ho jako "scratch"
    private static uint UnsafeUInt0 = 0;
}

/// <summary>
/// Pomocné uvolnění bufferů z SL API a bezpečné čtení Unicode stringů.
/// </summary>
internal static class SlApiBuffer
{
    public static string? ReadAndFreeUniString(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
        try { return Marshal.PtrToStringUni(ptr); }
        finally { TryFree(ptr); }
    }

    public static void TryFree(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return;
        try { Marshal.FreeCoTaskMem(ptr); } catch { /* ignore */ }
        try { Marshal.FreeHGlobal(ptr); } catch { /* ignore */ }
    }
}
