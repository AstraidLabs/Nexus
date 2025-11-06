using Nexus.Core.Abstractions;
using Nexus.Core.Interop;
using Nexus.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Nexus.Core.Services
{
    [SupportedOSPlatform("windows")]
    public sealed class KmsClientReader : IKmsClientReader
    {
        public KmsClientInfo? GetClientInfo()
        {
            using var h = SafeSlcHandle.Create();
            if (h.IsInvalid) return null;

            // Najdeme "nejlepší" SKU (Licensed pokud existuje)
            var app = new Guid("55c92734-d682-4d71-983e-d6ec3f16059f");
            if (!TryPickSku(h.DangerousGetHandle(), app, out var sku)) return null;

            // Hodnoty z Service i z SKU
            string? clientMachineId = ReadServiceString(h, "ClientMachineID");

            string? regName = ReadSkuString(h, sku, "KeyManagementServiceName");
            int? regPort = ReadSkuDword(h, sku, "KeyManagementServicePort");

            string? discName = ReadServiceString(h, "DiscoveredKeyManagementServiceName");
            int? discPort = ReadServiceDword(h, "DiscoveredKeyManagementServicePort");
            string? discIp = ReadServiceString(h, "DiscoveredKeyManagementServiceIpAddress");

            int? actInterval = ReadServiceDword(h, "VLActivationInterval");
            int? renInterval = ReadServiceDword(h, "VLRenewalInterval");

            string? lookupDomain = ReadServiceString(h, "KeyManagementServiceLookupDomain");

            bool? caching = null;
            // KMS host caching (na novějších systémech přes SKU/Service může být dostupné)
            var cachingVal = ReadServiceDword(h, "DisableKeyManagementServiceHostCaching");
            if (cachingVal.HasValue) caching = cachingVal.Value == 0; // 0 = enabled

            return new KmsClientInfo
            {
                ClientMachineId = clientMachineId,
                RegisteredKmsName = regName,
                RegisteredKmsPort = regPort,
                DiscoveredKmsName = discName,
                DiscoveredKmsPort = discPort,
                DiscoveredKmsIp = discIp,
                ActivationIntervalMinutes = actInterval,
                RenewalIntervalMinutes = renInterval,
                LookupDomain = lookupDomain,
                HostCachingEnabled = caching
            };
        }

        // --- helpers ---

        private static bool TryPickSku(IntPtr h, Guid app, out Guid sku)
        {
            sku = Guid.Empty;

            uint count = 0;
            var hr = Slapi.SLGetSLIDList(h, 0, ref app, 1, ref count, out var ids);
            if (hr != 0 || count == 0 || ids == IntPtr.Zero) return false;

            try
            {
                Guid? best = null;
                int bestStatus = 99;

                for (int i = 0; i < count; i++)
                {
                    var skuId = Marshal.PtrToStructure<Guid>(ids + i * 16);
                    uint cStatus = 0;
                    var hr2 = Slapi.SLGetLicensingStatusInformation(h, ref app, ref skuId, IntPtr.Zero, ref cStatus, out var pSt);
                    if (hr2 != 0 || cStatus == 0 || pSt == IntPtr.Zero) continue;

                    try
                    {
                        var raw = Marshal.ReadInt32(pSt, 16);
                        var norm = NormalizeStatus(raw);
                        if (best is null || (bestStatus != 1 && norm == 1))
                        {
                            best = skuId;
                            bestStatus = norm;
                            if (norm == 1) break;
                        }
                    }
                    finally { SlApiBuffer.TryFree(pSt); }
                }

                if (best is null) return false;
                sku = best.Value;
                return true;
            }
            finally { SlApiBuffer.TryFree(ids); }
        }

        private static int NormalizeStatus(int raw) => raw switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => 3,
            4 => 2,
            5 => 3,
            _ => 4
        };

        private static string? ReadSkuString(SafeSlcHandle h, Guid sku, string name)
        {
            var hr = Slapi.SLGetProductSkuInformation(h.DangerousGetHandle(), ref sku, name,
                out var type, out var cb, out var ptr);
            if (hr != 0 || cb == 0 || type != 1 || ptr == IntPtr.Zero) return null;
            return SlApiBuffer.ReadAndFreeUniString(ptr);
        }
        private static int? ReadSkuDword(SafeSlcHandle h, Guid sku, string name)
        {
            var hr = Slapi.SLGetProductSkuInformation(h.DangerousGetHandle(), ref sku, name,
                out var type, out var cb, out var ptr);
            if (hr != 0 || cb == 0 || type != 4 || ptr == IntPtr.Zero) return null;
            try { return Marshal.ReadInt32(ptr); }
            finally { SlApiBuffer.TryFree(ptr); }
        }
        private static string? ReadServiceString(SafeSlcHandle h, string name)
        {
            var hr = Slapi.SLGetServiceInformation(h.DangerousGetHandle(), name,
                out var type, out var cb, out var ptr);
            if (hr != 0 || cb == 0 || type != 1 || ptr == IntPtr.Zero) return null;
            return SlApiBuffer.ReadAndFreeUniString(ptr);
        }
        private static int? ReadServiceDword(SafeSlcHandle h, string name)
        {
            var hr = Slapi.SLGetServiceInformation(h.DangerousGetHandle(), name,
                out var type, out var cb, out var ptr);
            if (hr != 0 || cb == 0 || type != 4 || ptr == IntPtr.Zero) return null;
            try { return Marshal.ReadInt32(ptr); }
            finally { SlApiBuffer.TryFree(ptr); }
        }
    }
}
