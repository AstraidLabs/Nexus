using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Nexus.Core.Interop
{
    public static class Slapi
    {
        [DllImport("sppc.dll", EntryPoint = "SLOpen", ExactSpelling = true)]
        public static extern int SLOpen(out IntPtr phSLC);

        [DllImport("sppc.dll", EntryPoint = "SLClose", ExactSpelling = true)]
        public static extern int SLClose(IntPtr hSLC);

        [DllImport("sppc.dll", EntryPoint = "SLGetSLIDList", CharSet = CharSet.Unicode)]
        public static extern int SLGetSLIDList(
            IntPtr hSLC,
            uint eQuery,
            ref Guid appId,
            uint cReturnedTypes,
            ref uint pcReturnedIds,
            out IntPtr pReturnedIds);

        [DllImport("sppc.dll", EntryPoint = "SLGetLicensingStatusInformation", CharSet = CharSet.Unicode)]
        public static extern int SLGetLicensingStatusInformation(
            IntPtr hSLC,
            ref Guid appId,
            ref Guid skuId,
            IntPtr pValueNames,
            ref uint pcStatus,
            out IntPtr ppStatus);

        [DllImport("sppc.dll", EntryPoint = "SLGetPKeyInformation", CharSet = CharSet.Unicode)]
        public static extern int SLGetPKeyInformation(
            IntPtr hSLC,
            ref Guid pkeyId,
            string valueName,
            out uint peDataType,
            out uint pcbValue,
            out IntPtr ppbValue);

        [DllImport("sppc.dll", EntryPoint = "SLGetProductSkuInformation", CharSet = CharSet.Unicode)]
        public static extern int SLGetProductSkuInformation(
            IntPtr hSLC,
            ref Guid skuId,
            string valueName,
            out uint peDataType,
            out uint pcbValue,
            out IntPtr ppbValue);

        [DllImport("sppc.dll", EntryPoint = "SLIsWindowsGenuineLocal")]
        public static extern int SLIsWindowsGenuineLocal(out uint pdwGenuineState);

        [DllImport("sppc.dll", EntryPoint = "SLGetServiceInformation", CharSet = CharSet.Unicode)]
        public static extern int SLGetServiceInformation(
            IntPtr hSLC,
        string valueName,
        out uint peDataType,
        out uint pcbValue,
        out IntPtr ppbValue);

        [DllImport("slc.dll", EntryPoint = "SLGetWindowsInformationDWORD")]
        public static extern int SLGetWindowsInformationDWORD(string valueName, out uint pdwValue);

    }
}
