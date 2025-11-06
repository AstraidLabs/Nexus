namespace Nexus.Core.Abstractions
{
    /// <summary>
    /// Rozhraní pro zjištění, zda má systém aktivní digitální licenci (např. aktivace přes Microsoft Store nebo online účet).
    /// </summary>
    public interface IDigitalLicenseChecker
    {
        /// <summary>
        /// Vrátí true, pokud je přítomna digitální licence.
        /// False, pokud ne.
        /// Null, pokud nelze spolehlivě zjistit.
        /// </summary>
        bool? IsDigitalLicensePresent();
    }
}
