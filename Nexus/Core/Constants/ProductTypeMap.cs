using Nexus.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Constants
{
    public static class ProductTypeMap
    {
        public static readonly IReadOnlyDictionary<uint, string> Names = new Dictionary<uint, string>
    {
        { (uint)ProductType.Business, "Business" },
        { (uint)ProductType.Home, "Home" },
        { (uint)ProductType.Professional, "Professional" },
        { (uint)ProductType.Enterprise, "Enterprise" },
        { (uint)ProductType.EnterpriseN, "Enterprise N" },
        { (uint)ProductType.Education, "Education" },
        { (uint)ProductType.EnterpriseS, "Enterprise S" },
        { (uint)ProductType.ProfEducation, "Professional Education" },
        { (uint)ProductType.ProfEducationN, "Professional Education N" },
        { (uint)ProductType.Workstation, "Professional Workstation" },
        { (uint)ProductType.WorkstationN, "Professional Workstation N" },
        { (uint)ProductType.IoTEnterprise, "IoT Enterprise" },
        { (uint)ProductType.ProfessionalN, "Professional N" },
    };
    }
}
