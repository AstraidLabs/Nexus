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
        private readonly IWindowsSystemReader _sys;
        private readonly IKmsClientReader _kms;
        private readonly IAdvancedActivationReader _adv;
        private readonly ISubscriptionReader _sub;
        private readonly ISkuReader _sku;

        public WindowsInfoFacade()
            : this(new WindowsSystemReader(), new KmsClientReader(), new AdvancedActivationReader(), new SubscriptionReader(), new SkuReader())
        {
        }

        public WindowsInfoFacade(
            IWindowsSystemReader systemReader,
            IKmsClientReader kmsClientReader,
            IAdvancedActivationReader advancedActivationReader,
            ISubscriptionReader subscriptionReader,
            ISkuReader skuReader)
        {
            _sys = systemReader ?? throw new ArgumentNullException(nameof(systemReader));
            _kms = kmsClientReader ?? throw new ArgumentNullException(nameof(kmsClientReader));
            _adv = advancedActivationReader ?? throw new ArgumentNullException(nameof(advancedActivationReader));
            _sub = subscriptionReader ?? throw new ArgumentNullException(nameof(subscriptionReader));
            _sku = skuReader ?? throw new ArgumentNullException(nameof(skuReader));
        }

        public WindowsSystemData GetSystem() => _sys.GetSystemData();
        public KmsClientInfo? GetKmsClient() => _kms.GetClientInfo();
        public AdbaInfo? GetAdba() => _adv.GetAdbaInfo();
        public AvmaInfo? GetAvma() => _adv.GetAvmaInfo();
        public SubscriptionInfo GetSubscription() => _sub.GetSubscriptionInfo();
        public IReadOnlyList<SlSkuEntry> GetSkus() => _sku.GetAllSkus();
    }
}
