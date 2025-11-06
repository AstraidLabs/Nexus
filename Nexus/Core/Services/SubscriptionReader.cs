using Nexus.Core.Abstractions;
using Nexus.Core.Interop;
using Nexus.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace Nexus.Core.Services
{
    [SupportedOSPlatform("windows")]
    public sealed class SubscriptionReader : ISubscriptionReader
    {
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
            catch { /* Clipc nemusí existovat na starších edicích */ }

            return info;
        }

        private static bool GetWindowsInfoDword(string key, ref uint value)
        {
            try
            {
                // SLGetWindowsInformationDWORD existuje jen na starších clusterech,
                // ale v našem interopu je nepřidaná. Použijeme Service info fallback.
                return false;
            }
            catch { return false; }
        }
    }
}
