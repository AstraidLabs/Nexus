using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Models;

public sealed class KmsClientInfo
{
    public string? ClientMachineId { get; init; }
    public string? RegisteredKmsName { get; init; }
    public int? RegisteredKmsPort { get; init; }
    public string? DiscoveredKmsName { get; init; }
    public int? DiscoveredKmsPort { get; init; }
    public string? DiscoveredKmsIp { get; init; }
    public int? ActivationIntervalMinutes { get; init; }
    public int? RenewalIntervalMinutes { get; init; }
    public string? LookupDomain { get; init; }
    public bool? HostCachingEnabled { get; init; } // null = neznámo
}