using System.Security.Cryptography.X509Certificates;

namespace eMandates.Merchant.Library.Configuration
{
    /// <summary>
    /// Interface that describes the configuration settings for the library, which are tied with each ICommunicator instance:
    /// when you instantiate a Communicator object, it attempts to load its configuration using App.config or Web.config.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// eMandate.ContractID as supplied to you by the creditor bank.
        /// If the eMandate.ContractID has less than 9 digits, use leading zeros to fill out the field.
        /// </summary>
        string EMandateContractId { get; }

        /// <summary>
        /// eMandate.ContractSubId as supplied to you by the creditor bank.
        /// If you do not have a ContractSubId, use 0 for this field.
        /// </summary>
        uint EMandateContractSubId { get; }

        /// <summary>
        /// A valid URL to which the debtor banks redirects to, after the debtor has authorized a transaction.
        /// </summary>
        string MerchantReturnUrl { get; }

        /// <summary>
        /// A string which specifies the fingerprint of the certificate to use to sign messages to the creditor bank.
        /// </summary>
        string SigningCertificateFingerprint { get; }

        /// <summary>
        /// A string which specifies the fingerprint of the certificate to use to validate messages from the creditor bank.
        /// </summary>
        string AcquirerCertificateFingerprint { get; }

        /// <summary>
        /// A string which specifies the fingerprint of the second certificate to use to validate messages from the creditor bank.
        /// </summary>
        string AcquirerAlternateCertificateFingerprint { get; }

        /// <summary>
        /// You may overwrite the signing certificate (which was loaded using the fingerprint specified using SigningCertificateFingerprint), if you want
        /// to load it using a different method.
        /// </summary>
        X509Certificate2 SigningCertificate { get; }

        /// <summary>
        /// You may overwrite the acquirer certificate (which was loaded using the fingerprint specified using AcquirerCertificateFingerprint), if you want
        /// to load it using a different method.
        /// </summary>
        X509Certificate2 AcquirerCertificate { get; }

        /// <summary>
        /// You may overwrite the acquirer second certificate (which was loaded using the fingerprint specified using AcquirerCertificateFingerprint), if you want
        /// to load it using a different method.
        /// </summary>
        X509Certificate2 AcquirerAlternateCertificate { get; }

        /// <summary>
        /// The URL to which the library sends Directory request messages
        /// </summary>
        string AcquirerUrlDirectoryReq { get; }

        /// <summary>
        /// The URL to which the library sends Transaction messages (including eMandates messages).
        /// </summary>
        string AcquirerUrlTransactionReq { get; }

        /// <summary>
        /// The URL to which the library sends Status request messages
        /// </summary>
        string AcquirerUrlStatusReq { get; }

        /// <summary>
        /// This tells the library that it should save ISO pain raw messages or not. Default is true.
        /// </summary>
        bool ServiceLogsEnabled { get; }

        /// <summary>
        /// A directory on the disk where the library saves ISO pain raw messages.
        /// </summary>
        string ServiceLogsLocation { get; }

        /// <summary>
        /// A string that describes a pattern to distinguish the ISO pain raw messages. For example,
        /// %Y-%M-%D\%h%m%s.%f-%a.xml -> 102045.924-AcquirerTrxReq.xml
        /// </summary>
        /// <remarks>
        /// %Y = current year
        /// %M = current month
        /// %D = current day
        /// %h = current hour
        /// %m = current minute
        /// %s = current second
        /// %f = current millisecond
        /// %a = current action
        /// </remarks>
        string ServiceLogsPattern { get; }

        /// <summary>
        /// ILoggerFactory instance that is used to create ILogger object
        /// </summary>
        ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// An object you can use to load certificates by Fingerprint. By default, it loads from the Windows
        /// certificate store.
        /// </summary>
        ICertificateLoader CertificateLoader { get; }
    }
}
