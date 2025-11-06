using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Models
{
    public sealed class WindowsSystemData
    {
        public string Edition { get; init; } = "";
        public string Version { get; init; } = "";
        public string Architecture { get; init; } = "";
        public ActivationInfo Activation { get; init; } = new();
        public ProductInfo Product { get; init; } = new();
    }
}
