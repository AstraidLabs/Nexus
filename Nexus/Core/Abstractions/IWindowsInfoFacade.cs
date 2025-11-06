using Nexus.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Core.Abstractions
{
    public interface IWindowsInfoFacade
    {
        WindowsSystemData GetSystem();
        KmsClientInfo? GetKmsClient();
        AdbaInfo? GetAdba();
        AvmaInfo? GetAvma();
        SubscriptionInfo GetSubscription();
        IReadOnlyList<SlSkuEntry> GetSkus();
    }
}
