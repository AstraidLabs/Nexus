using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace Nexus.Core.Interop
{
    internal static class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        internal static extern bool GetProductInfo(
            int dwOSMajorVersion,
            int dwOSMinorVersion,
            int dwSpMajorVersion,
            int dwSpMinorVersion,
            out uint pdwReturnedProductType);
    }
}
