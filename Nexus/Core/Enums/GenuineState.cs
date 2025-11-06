using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Enums
{
    public enum GenuineState
    {
        Genuine = 0,
        InvalidLicense = 1,
        Tampered = 2,
        Offline = 3,
        Last = 4
    }
}
