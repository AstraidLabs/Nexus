using System;
using System.Runtime.Versioning;
using Nexus.Core.Abstractions;
using Nexus.Core.Interop;

namespace Nexus.Core.Services
{
    /// <summary>
    /// Služba pro detekci digitální licence Windows.
    /// Využívá COM objekt EditionUpgradeManagerObj.EditionUpgradeManager.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class DigitalLicenseChecker : IDigitalLicenseChecker
    {
        /// <summary>
        /// Pokusí se zjistit, zda má aktuální systém aktivní digitální licenci.
        /// </summary>
        /// <returns>
        /// True, pokud digitální licence existuje.
        /// False, pokud ne.
        /// Null, pokud nelze stav zjistit (např. chybějící COM nebo výjimka).
        /// </returns>
        public bool? IsDigitalLicensePresent()
        {
            try
            {
                // Získej COM instanci EditionUpgradeManager.
                var eum = EumFactory.Create();
                if (eum == null)
                {
                    return null; // COM není k dispozici (pravděpodobně odinstalován Store nebo chybí API).
                }

                // Zavolej metodu AcquireModernLicenseForWindows.
                var hr = eum.AcquireModernLicenseForWindows(1, out var rc);
                if (hr != 0)
                {
                    return null; // HRESULT != S_OK → nelze spolehlivě zjistit.
                }

                // Interpretace návratového kódu (empiricky zjištěné).
                //  rc >= 0 → licence přítomna.
                //  rc == 1 → chybí.
                if (rc == 1)
                {
                    return false;
                }

                return rc >= 0;
            }
            catch (Exception)
            {
                // Fail-safe – nepropaguj výjimky.
                return null;
            }
        }
    }
}
