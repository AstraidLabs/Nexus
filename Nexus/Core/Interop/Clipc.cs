using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Nexus.Core.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ClipcSubStatus
    {
        public uint Enabled;
        public uint Sku;
        public uint State;
    }

    internal static class Clipc
    {
        [DllImport("Clipc.dll", ExactSpelling = true)]
        internal static extern int ClipGetSubscriptionStatus(out ClipcSubStatus pStatus);
    }

}
