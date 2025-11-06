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
    public sealed class AdvancedActivationReader : IAdvancedActivationReader
    {
        public AdbaInfo? GetAdbaInfo()
        {
            using var h = SafeSlcHandle.Create();
            if (h.IsInvalid) return null;

            var app = new Guid("55c92734-d682-4d71-983e-d6ec3f16059f");
            if (!TryPickSku(h.DangerousGetHandle(), app, out var sku)) return null;

            return new AdbaInfo
            {
                ObjectName = ReadSkuString(h, sku, "ADActivationObjectName"),
                ObjectDn = ReadSkuString(h, sku, "ADActivationObjectDN"),
                CsvlkPid = ReadSkuString(h, sku, "ADActivationCsvlkPID"),
                CsvlkSkuId = ReadSkuString(h, sku, "ADActivationCsvlkSkuID")
            };
        }

        public AvmaInfo? GetAvmaInfo()
        {
            using var h = SafeSlcHandle.Create();
            if (h.IsInvalid) return null;

            var app = new Guid("55c92734-d682-4d71-983e-d6ec3f16059f");
            if (!TryPickSku(h.DangerousGetHandle(), app, out var sku)) return null;

            var ticks = ReadSkuInt64(h, sku, "InheritedActivationActivationTime");

            return new AvmaInfo
            {
                HostMachineName = ReadSkuString(h, sku, "InheritedActivationHostMachineName"),
                HostDigitalPid2 = ReadSkuString(h, sku, "InheritedActivationHostDigitalPid2"),
                InheritedActivationId = ReadSkuString(h, sku, "InheritedActivationId"),
                ActivationTime = ticks.HasValue && ticks.Value > 0
                    ? DateTimeOffset.FromFileTime(ticks.Value)
                    : null
            };
        }

        // shared helpers
        private static bool TryPickSku(IntPtr h, Guid app, out Guid sku)
        {
            sku = Guid.Empty; uint count = 0;
            var hr = Slapi.SLGetSLIDList(h, 0, ref app, 1, ref count, out var ids);
            if (hr != 0 || count == 0 || ids == IntPtr.Zero) return false;
            try { sku = Marshal.PtrToStructure<Guid>(ids); return true; }
            finally { SlApiBuffer.TryFree(ids); }
        }
        private static string? ReadSkuString(SafeSlcHandle h, Guid sku, string name)
        {
            var hr = Slapi.SLGetProductSkuInformation(h.DangerousGetHandle(), ref sku, name,
                out var type, out var cb, out var ptr);
            if (hr != 0 || cb == 0 || type != 1 || ptr == IntPtr.Zero) return null;
            return SlApiBuffer.ReadAndFreeUniString(ptr);
        }
        private static long? ReadSkuInt64(SafeSlcHandle h, Guid sku, string name)
        {
            var hr = Slapi.SLGetProductSkuInformation(h.DangerousGetHandle(), ref sku, name,
                out var type, out var cb, out var ptr);
            if (hr != 0 || cb == 0 || type != 3 || ptr == IntPtr.Zero) return null;
            try { return Marshal.ReadInt64(ptr); }
            finally { SlApiBuffer.TryFree(ptr); }
        }
    }
}
