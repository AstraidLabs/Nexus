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
    public sealed class SkuReader : ISkuReader
    {
        public IReadOnlyList<SlSkuEntry> GetAllSkus()
        {
            var result = new List<SlSkuEntry>();
            using var h = SafeSlcHandle.Create();
            if (h.IsInvalid) return result;

            var app = new Guid("55c92734-d682-4d71-983e-d6ec3f16059f");
            uint count = 0;
            var hr = Slapi.SLGetSLIDList(h.DangerousGetHandle(), 0, ref app, 1, ref count, out var ids);
            if (hr != 0 || count == 0 || ids == IntPtr.Zero) return result;

            try
            {
                for (int i = 0; i < count; i++)
                {
                    var sku = Marshal.PtrToStructure<Guid>(ids + i * 16);
                    var name = ReadSkuString(h, sku, "Name");
                    var desc = ReadSkuString(h, sku, "Description");
                    var ch = ReadSkuString(h, sku, "Channel");

                    string? ppk = null;
                    // PartialProductKey je přes PKey API, ale většinou funguje i přes SKU:
                    ppk = ReadPKeyString(h, sku, "PartialProductKey") ?? ppk;

                    result.Add(new SlSkuEntry
                    {
                        SkuId = sku,
                        Name = name,
                        Description = desc,
                        Channel = ch,
                        PartialProductKey = ppk
                    });
                }
            }
            finally { SlApiBuffer.TryFree(ids); }

            return result;
        }

        private static string? ReadSkuString(SafeSlcHandle h, Guid sku, string name)
        {
            var hr = Slapi.SLGetProductSkuInformation(h.DangerousGetHandle(), ref sku, name,
                out var type, out var cb, out var ptr);
            if (hr != 0 || cb == 0 || type != 1 || ptr == IntPtr.Zero) return null;
            return SlApiBuffer.ReadAndFreeUniString(ptr);
        }
        private static string? ReadPKeyString(SafeSlcHandle h, Guid id, string name)
        {
            var hr = Slapi.SLGetPKeyInformation(h.DangerousGetHandle(), ref id, name,
                out var type, out var cb, out var ptr);
            if (hr != 0 || cb == 0 || type != 1 || ptr == IntPtr.Zero) return null;
            return SlApiBuffer.ReadAndFreeUniString(ptr);
        }
    }
}
