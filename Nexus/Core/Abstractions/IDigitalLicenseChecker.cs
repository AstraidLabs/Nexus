using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Abstractions
{
    public interface IDigitalLicenseChecker
    {
        bool? IsDigitalLicensePresent(); // null = nelze zjistit bezpečně
    }
}
