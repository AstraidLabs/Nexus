using Nexus.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Abstractions
{
    public interface IWindowsSystemReader
    {
        WindowsSystemData GetSystemData();          // vše na jednom místě
        ActivationInfo GetActivationInfo();         // stav licence (read-only)
        ProductInfo GetProductInfo();               // edice, release, typ
        PKeyInfo? GetInstalledProductKeyPartial();  // posledních 5 znaků, pokud existuje
    }
}
