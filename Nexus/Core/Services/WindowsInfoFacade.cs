using Nexus.Core.Abstractions;
using Nexus.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace Nexus.Core.Services
{
    [SupportedOSPlatform("windows")]
    public sealed class WindowsInfoFacade : IWindowsInfoFacade
    {
        private readonly IWindowsSystemReader _sys = new WindowsSystemReader();
        private readonly IKmsClientReader _kms = new KmsClientReader();
        private readonly IAdvancedActivationReader _adv = new AdvancedActivationReader();
        private readonly ISubscriptionReader _sub = new SubscriptionReader();
        private readonly ISkuReader _sku = new SkuReader();

        public WindowsSystemData GetSystem() => _sys.GetSystemData();
        public KmsClientInfo? GetKmsClient() => _kms.GetClientInfo();
        public AdbaInfo? GetAdba() => _adv.GetAdbaInfo();
        public AvmaInfo? GetAvma() => _adv.GetAvmaInfo();
        public SubscriptionInfo GetSubscription() => _sub.GetSubscriptionInfo();
        public IReadOnlyList<SlSkuEntry> GetSkus() => _sku.GetAllSkus();
    }
}
