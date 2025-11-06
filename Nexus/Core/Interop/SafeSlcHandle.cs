using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace Nexus.Core.Interop
{
    public sealed class SafeSlcHandle : SafeHandle
    {
        public SafeSlcHandle() : base(IntPtr.Zero, true) { }
        public override bool IsInvalid => handle == IntPtr.Zero;
        protected override bool ReleaseHandle() => Slapi.SLClose(handle) == 0;

        public static SafeSlcHandle Create()
        {
            var hr = Slapi.SLOpen(out var h);
            if (hr != 0 || h == IntPtr.Zero) return new SafeSlcHandle(); // invalid
            var sh = new SafeSlcHandle();
            sh.SetHandle(h);
            return sh;
        }
    }
}
