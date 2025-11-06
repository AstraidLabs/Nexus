using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Models
{
    public sealed class AvmaInfo
    {
        public string? HostMachineName { get; init; }
        public string? HostDigitalPid2 { get; init; }
        public string? InheritedActivationId { get; init; }
        public DateTimeOffset? ActivationTime { get; init; }
    }
}
