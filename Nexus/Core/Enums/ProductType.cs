using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Enums
{
    public enum ProductType : uint
    {
        Undefined = 0x00000000,
        Business = 0x00000006,
        Home = 0x00000010,
        Professional = 0x00000012,
        Enterprise = 0x00000027,
        EnterpriseN = 0x0000002A,
        Education = 0x00000030,
        EnterpriseS = 0x0000003C,
        ProfEducation = 0x0000003F,
        ProfEducationN = 0x00000040,
        Workstation = 0x00000043,
        WorkstationN = 0x00000044,
        IoTEnterprise = 0x0000004B,
        ProfessionalN = 0x00000065
    }
}
