using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Nexus.Core.Interop
{
    /// <summary>
    /// COM rozhraní EditionUpgradeManagerObj – interně používané systémem pro kontrolu digitální licence.
    /// </summary>
    [ComImport, Guid("F2DCB80D-0670-44BC-9002-CD18688730AF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEum
    {
        // Neznámé metody v tabulce virtuálních funkcí (nejsou použity).
        void VF1();
        void VF2();
        void VF3();
        void VF4();

        /// <summary>
        /// Získá stav moderní digitální licence Windows.
        /// </summary>
        /// <param name="param">Vstupní parametr (obvykle 1).</param>
        /// <param name="returnCode">Výstupní kód licence (0 = OK, jiné hodnoty = různé stavy).</param>
        /// <returns>HRESULT – 0 při úspěchu.</returns>
        int AcquireModernLicenseForWindows(int param, out int returnCode);
    }

    /// <summary>
    /// Pomocná továrna pro vytvoření COM instance EditionUpgradeManagerObj.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class EumFactory
    {
        private const string ProgId = "EditionUpgradeManagerObj.EditionUpgradeManager";

        /// <summary>
        /// Pokusí se vytvořit instanci COM objektu EditionUpgradeManager.
        /// </summary>
        public static IEum? Create()
        {
            var t = Type.GetTypeFromProgID(ProgId, throwOnError: false);
            return t == null ? null : (IEum?)Activator.CreateInstance(t);
        }
    }
}
