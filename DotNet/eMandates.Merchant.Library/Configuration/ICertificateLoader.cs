using System.Security.Cryptography.X509Certificates;

namespace eMandates.Merchant.Library.Configuration
{
    /// <summary>
    /// Certificate loader interface, implement this if you want to provide your own loader
    /// </summary>
    public interface ICertificateLoader
    {
        /// <summary>
        /// Loads the certificate, and returns it
        /// </summary>
        /// <param name="fingerprint">The fingerprint to use when searching the certificate</param>
        /// <returns></returns>
        X509Certificate2 Load(string fingerprint);
    }
}
