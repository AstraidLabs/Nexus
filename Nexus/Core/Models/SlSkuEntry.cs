using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Models
{
    public sealed class SlSkuEntry
    {
        public Guid SkuId { get; init; }
        public string? Name { get; init; }
        public string? Description { get; init; }
        public string? Channel { get; init; }
        public string? PartialProductKey { get; init; }
    }
}
