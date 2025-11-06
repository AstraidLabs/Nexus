using Nexus.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Models
{
    public sealed class ActivationInfo
    {
        public LicensingStatus Status { get; init; }
        public string? StatusMessage { get; init; }
        public GenuineState Genuine { get; init; }
        public int? GraceMinutesRemaining { get; init; }
        public DateTimeOffset? EvaluationEndUtc { get; init; }
        public string? Channel { get; init; }                 // Retail/KMS/MAK/...
        public string? PartialProductKey { get; init; }       // posledních 5 znaků
        public VolumeActivationType? VlActivationType { get; init; }
        public string? ExtendedPid { get; init; }  // DigitalPID (ze SLGetPKeyInformation)
        public string? ProductId { get; init; }  // DigitalPID2 (když jde přečíst)
    }
}
