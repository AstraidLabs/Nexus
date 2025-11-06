using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Models
{
    public sealed class PKeyInfo
    {
        public string? PartialProductKey { get; init; }
        public string? Channel { get; init; }
        public string? DigitalPid { get; init; }
    }
}
