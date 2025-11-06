using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Nexus.Core.Abstractions;
using Nexus.Core.Interop;
using Nexus.Core.Models;

namespace Nexus.Core.Services
{
    /// <summary>
    /// Čte informace o Windows předplatném pomocí Software Licensing API a souvisejících služeb.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed partial class SubscriptionReader : ISubscriptionReader
    {
        /// <inheritdoc />
        public SubscriptionInfo GetSubscriptionInfo()
        {
            var info = new SubscriptionInfo { Supported = false, Enabled = false };

            // Zjisti podporu přes policy DWORD (název se liší podle buildů)
            uint dw = 0;
            bool supported = GetWindowsInfoDword("Allow-WindowsSubscription", ref dw)
                          || GetWindowsInfoDword("ConsumeAddonPolicySet", ref dw);

            info = info with { Supported = supported && dw != 0 };

            // Clipc – stav subscripce
            try
            {
                var hr = Clipc.ClipGetSubscriptionStatus(out var st);
                if (hr == 0)
                {
                    info = info with
                    {
                        Enabled = st.Enabled != 0,
                        Sku = st.Sku,
                        State = st.State
                    };
                }
            }
            catch
            {
                // Clipc nemusí existovat na starších edicích.
            }

            return info;
        }

        /// <summary>
        /// Pokusí se přečíst DWORD hodnotu z Windows Licensing subsystému.
        /// Používá SLGetWindowsInformationDWORD a fallback na SLGetWindowsInformation.
        /// </summary>
        /// <param name="key">Název hodnoty (např. "Allow-WindowsSubscription").</param>
        /// <param name="value">Výstupní hodnota DWORD.</param>
        /// <returns>True, pokud byla hodnota úspěšně načtena; jinak false.</returns>
        private static bool GetWindowsInfoDword(string key, ref uint value)
        {
            try
            {
                // Primární pokus přes SLGetWindowsInformationDWORD.
                var hr = Slapi.SLGetWindowsInformationDWORD(key, out value);
                if (hr == 0)
                {
                    return true;
                }

                // Fallback – použij univerzální API pro čtení hodnot.
                uint type = 0;
                uint cb = 0;
                IntPtr ptr = IntPtr.Zero;
                hr = Slapi.SLGetWindowsInformation(key, out type, out cb, out ptr);
                if (hr != 0 || ptr == IntPtr.Zero)
                {
                    return false;
                }

                try
                {
                    // Ověř, že jde o DWORD (typ = 4) a velikost je alespoň 4 bajty.
                    if (type == 4 && cb >= sizeof(uint))
                    {
                        value = (uint)Marshal.ReadInt32(ptr);
                        return true;
                    }

                    return false;
                }
                finally
                {
                    // Vždy uvolni nativní paměť (i při výjimce).
                    SlApiBuffer.TryFree(ptr);
                }
            }
            catch
            {
                // Fail-safe: nepropaguj výjimky ven.
                return false;
            }
        }
    }
}
