using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Models
{
    public sealed class AdbaInfo
    {
        public string? ObjectName { get; init; }
        public string? ObjectDn { get; init; }
        public string? CsvlkPid { get; init; }
        public string? CsvlkSkuId { get; init; }
    }
}
