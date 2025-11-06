using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Models
{
    public sealed record class SubscriptionInfo
    {
        public bool Supported { get; init; }
        public bool Enabled { get; init; }
        public uint? Sku { get; init; }
        public uint? State { get; init; }
    }
}
