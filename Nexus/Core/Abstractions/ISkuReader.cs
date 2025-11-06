using Nexus.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Abstractions
{
    public interface ISkuReader
    {
        IReadOnlyList<SlSkuEntry> GetAllSkus(); // Windows AppId
    }
}
