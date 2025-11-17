using Nexus.Core.Abstractions;
using Nexus.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace Nexus.Core.Services
{
    /// <summary>
    /// Fasáda spojující všechny specializované čtečky do jednoho rozhraní <see cref="IWindowsInfoFacade"/>.
    /// V aplikační vrstvě tak stačí pracovat s jedním objektem, který interně deleguje čtení
    /// systémových informací, licencování a SKU na jednotlivé služby.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class WindowsInfoFacade : IWindowsInfoFacade
    {
        private readonly IWindowsSystemReader _sys;
        private readonly IKmsClientReader _kms;
        private readonly IAdvancedActivationReader _adv;
        private readonly ISubscriptionReader _sub;
        private readonly ISkuReader _sku;

        /// <summary>
        /// Výchozí konstruktor využívající produkční implementace všech čteček.
        /// Díky tomu je možné instanci použít okamžitě, ale zároveň zůstává prostor
        /// pro injektování vlastních readerů například v testech.
        /// </summary>
        public WindowsInfoFacade()
            : this(new WindowsSystemReader(), new KmsClientReader(), new AdvancedActivationReader(), new SubscriptionReader(), new SkuReader())
        {
        }

        /// <summary>
        /// Vytvoří fasádu s explicitně dodanými implementacemi jednotlivých readerů.
        /// Každý reader zodpovídá za čtení jedné domény (systém, KMS, ADBA/AVMA, předplatné, SKU)
        /// a fasáda je pouze skládá dohromady do jednotného API.
        /// </summary>
        /// <param name="systemReader">Čtečka základních systémových údajů o Windows.</param>
        /// <param name="kmsClientReader">Čtečka informací o KMS klientovi.</param>
        /// <param name="advancedActivationReader">Čtečka pokročilých aktivačních dat (ADBA/AVMA).</param>
        /// <param name="subscriptionReader">Čtečka informací o předplatném.</param>
        /// <param name="skuReader">Čtečka všech dostupných SKU.</param>
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

        /// <summary>
        /// Načte obecné systémové informace včetně vydání a architektury.
        /// </summary>
        public WindowsSystemData GetSystem() => _sys.GetSystemData();

        /// <summary>
        /// Vrátí metadata o KMS klientovi (pokud je k dispozici).
        /// </summary>
        public KmsClientInfo? GetKmsClient() => _kms.GetClientInfo();

        /// <summary>
        /// Načte stav ADBA služby.
        /// </summary>
        public AdbaInfo? GetAdba() => _adv.GetAdbaInfo();

        /// <summary>
        /// Načte stav AVMA aktivace pro virtualizační scénáře.
        /// </summary>
        public AvmaInfo? GetAvma() => _adv.GetAvmaInfo();

        /// <summary>
        /// Vrací informace o předplatném a jeho podpoře na daném zařízení.
        /// </summary>
        public SubscriptionInfo GetSubscription() => _sub.GetSubscriptionInfo();

        /// <summary>
        /// Vrací seznam všech známých SKU v systému včetně jejich stavů.
        /// </summary>
        public IReadOnlyList<SlSkuEntry> GetSkus() => _sku.GetAllSkus();
    }
}
