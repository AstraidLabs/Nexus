using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Constants
{
    public static class WellKnown
    {
        // AppID pro Windows SLAPI (neaktivuj, jen čti)
        public const string WindowsAppId = "55c92734-d682-4d71-983e-d6ec3f16059f";

        // Registry paths
        public const string SpPlatform = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform";
        public const string SlKeyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SL";
        public const string NsKeyPath = @"HKEY_USERS\S-1-5-20\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SL";

        // Common value names
        public const string KmsName = "KeyManagementServiceName";
        public const string KmsPort = "KeyManagementServicePort";
    }
}
