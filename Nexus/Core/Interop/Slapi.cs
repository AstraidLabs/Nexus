using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Nexus.Core.Interop
{
    /// <summary>
    /// SLAPI – Software Licensing API – volání poskytovaná knihovnami sppc.dll / slc.dll.
    /// Slouží k získávání informací o stavu licence systému Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class Slapi
    {
        /// <summary>
        /// Otevře sezení se Software Licensing Client (SLC) a vrátí handle pro další volání.
        /// </summary>
        [DllImport("sppc.dll", EntryPoint = "SLOpen", ExactSpelling = true)]
        internal static extern int SLOpen(out IntPtr phSLC);

        /// <summary>
        /// Uzavře dříve otevřené sezení SLC a uvolní související prostředky.
        /// </summary>
        [DllImport("sppc.dll", EntryPoint = "SLClose", ExactSpelling = true)]
        internal static extern int SLClose(IntPtr hSLC);

        /// <summary>
        /// Vrátí seznam identifikátorů SLID podle zadaného dotazu.
        /// </summary>
        [DllImport("sppc.dll", EntryPoint = "SLGetSLIDList", CharSet = CharSet.Unicode)]
        internal static extern int SLGetSLIDList(
            IntPtr hSLC,
            uint eQuery,
            ref Guid appId,
            uint cReturnedTypes,
            ref uint pcReturnedIds,
            out IntPtr pReturnedIds);

        /// <summary>
        /// Získá podrobné informace o stavu licence.
        /// </summary>
        [DllImport("sppc.dll", EntryPoint = "SLGetLicensingStatusInformation", CharSet = CharSet.Unicode)]
        internal static extern int SLGetLicensingStatusInformation(
            IntPtr hSLC,
            ref Guid appId,
            ref Guid skuId,
            IntPtr pValueNames,
            ref uint pcStatus,
            out IntPtr ppStatus);

        /// <summary>
        /// Načte informace o product key (PKey) identifikované GUIDem.
        /// </summary>
        [DllImport("sppc.dll", EntryPoint = "SLGetPKeyInformation", CharSet = CharSet.Unicode)]
        internal static extern int SLGetPKeyInformation(
            IntPtr hSLC,
            ref Guid pkeyId,
            string valueName,
            out uint peDataType,
            out uint pcbValue,
            out IntPtr ppbValue);

        /// <summary>
        /// Čte detaily produktu podle SKU identifikátoru.
        /// </summary>
        [DllImport("sppc.dll", EntryPoint = "SLGetProductSkuInformation", CharSet = CharSet.Unicode)]
        internal static extern int SLGetProductSkuInformation(
            IntPtr hSLC,
            ref Guid skuId,
            string valueName,
            out uint peDataType,
            out uint pcbValue,
            out IntPtr ppbValue);

        /// <summary>
        /// Určuje, zda je systém označen jako genuinní v lokálním hodnocení.
        /// </summary>
        [DllImport("sppc.dll", EntryPoint = "SLIsWindowsGenuineLocal")]
        internal static extern int SLIsWindowsGenuineLocal(out uint pdwGenuineState);

        /// <summary>
        /// Čte obecné servisní informace o licencování.
        /// </summary>
        [DllImport("sppc.dll", EntryPoint = "SLGetServiceInformation", CharSet = CharSet.Unicode)]
        internal static extern int SLGetServiceInformation(
            IntPtr hSLC,
            string valueName,
            out uint peDataType,
            out uint pcbValue,
            out IntPtr ppbValue);

        /// <summary>
        /// Načte DWORD hodnotu z Windows Software Licensing subsystému.
        /// Např. hodnoty typu "Allow-WindowsSubscription" nebo "ConsumeAddonPolicySet".
        /// </summary>
        [DllImport("slc.dll", EntryPoint = "SLGetWindowsInformationDWORD", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int SLGetWindowsInformationDWORD(
            string valueName,
            out uint pdwValue);

        /// <summary>
        /// Univerzální fallback metoda pro čtení libovolných hodnot (string, dword, qword, ...).
        /// Vrací pointer na nativní buffer, který je nutné uvolnit pomocí Marshal.FreeCoTaskMem.
        /// </summary>
        [DllImport("slc.dll", EntryPoint = "SLGetWindowsInformation", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int SLGetWindowsInformation(
            string valueName,
            out uint peDataType,
            out uint pcbValue,
            out IntPtr ppbValue);
    }
}
