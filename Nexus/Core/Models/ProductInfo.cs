using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Models
{
    public sealed class ProductInfo
    {
        public uint ProductTypeCode { get; init; }
        public string ProductTypeName { get; init; } = "";
        public string Release { get; init; } = ""; // např. 10.0.22631
    }
}
