using System;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using eMandates.Merchant.Library.AppConfig;

namespace eMandates.Merchant.Library.Configuration
{
    /// <summary>
    /// Configuration class
    /// </summary>
    public class Configuration : IConfiguration
    {
        /// <summary>
        /// Parameterless constructor
        /// </summary>
        internal Configuration()
        {
        }

        /// <summary>
        /// Constructor that highlights all required fields for this object
        /// </summary>
        public Configuration(string eMandateContractId, string merchantReturnUrl, string signingCertificateFingerprint, string acquirerCertificateFingerprint, string acquirerUrlDirectoryReq, string acquirerUrlTransactionReq,
            string acquirerUrlStatusReq, bool serviceLogsEnabled, string serviceLogsLocation, ILoggerFactory loggerFactory = null, ICertificateLoader certificateLoader = null, string serviceLogsPattern = null, uint eMandateContractSubId = 0)
            : this(eMandateContractId, merchantReturnUrl, signingCertificateFingerprint, acquirerCertificateFingerprint, null, acquirerUrlDirectoryReq, acquirerUrlTransactionReq, acquirerUrlStatusReq, serviceLogsEnabled, serviceLogsLocation, loggerFactory, certificateLoader, serviceLogsPattern, eMandateContractSubId)
        {
        }

        /// <summary>
        /// Constructor that highlights all required fields for this object
        /// </summary>
        public Configuration(string eMandateContractId, string merchantReturnUrl, string signingCertificateFingerprint, string acquirerCertificateFingerprint, string acquirerAlternateCertificateFingerprint, string acquirerUrlDirectoryReq, string acquirerUrlTransactionReq,
            string acquirerUrlStatusReq, bool serviceLogsEnabled, string serviceLogsLocation, ILoggerFactory loggerFactory = null, ICertificateLoader certificateLoader = null, string serviceLogsPattern = null, uint eMandateContractSubId = 0)
        {
            this.EMandateContractId = eMandateContractId;
            this.EMandateContractSubId = eMandateContractSubId;
            this.MerchantReturnUrl = merchantReturnUrl;
            this.SigningCertificateFingerprint = signingCertificateFingerprint;
            this.AcquirerCertificateFingerprint = acquirerCertificateFingerprint;
            this.AcquirerAlternateCertificateFingerprint = acquirerAlternateCertificateFingerprint;
            this.AcquirerUrlDirectoryReq = acquirerUrlDirectoryReq;
            this.AcquirerUrlTransactionReq = acquirerUrlTransactionReq;
            this.AcquirerUrlStatusReq = acquirerUrlStatusReq;

            this.ServiceLogsEnabled = serviceLogsEnabled;
            this.ServiceLogsLocation = serviceLogsLocation;
            this.ServiceLogsPattern = string.IsNullOrWhiteSpace(serviceLogsPattern) ? @"%Y-%M-%D\%h%m%s.%f-%a.xml" : serviceLogsPattern;

            this.LoggerFactory = loggerFactory ?? new LoggerFactory();

            this.CertificateLoader = certificateLoader ?? new CertificateStoreLoader();
            this.SigningCertificate = CertificateLoader.Load(SigningCertificateFingerprint);
            this.AcquirerCertificate = CertificateLoader.Load(AcquirerCertificateFingerprint);
            this.AcquirerAlternateCertificate = CertificateLoader.Load(AcquirerAlternateCertificateFingerprint);
        }

        /// <summary>
        /// Constructor that highlights all required fields for this object
        /// </summary>
        public Configuration(ApplicationSettings applicationSettings, ILoggerFactory loggerFactory = null, ICertificateLoader certificateLoader = null) : this (
            applicationSettings.ContractId,
            applicationSettings.MerchantReturnUrl,
            applicationSettings.SigningCertificateThumbprint,
            applicationSettings.AcquirerCertificateThumbprint,
            applicationSettings.AcquirerAlternateCertificateThumbprint,
            applicationSettings.AcquirerDirectoryRequestUrl,
            applicationSettings.AcquirerTransactionRequestUrl,
            applicationSettings.AcquirerStatusRequestUrl,
            applicationSettings.ServiceLogsEnabled,
            applicationSettings.ServiceLogsLocation,
            loggerFactory,
            certificateLoader,
            applicationSettings.ServiceLogsPattern,
            applicationSettings.ContractSubId
            )
        {
        }

        /// <summary>
        /// eMandate.ContractID as supplied to you by the creditor bank.
        /// If the eMandate.ContractID has less than 9 digits, use leading zeros to fill out the field.
        /// </summary>
        public string EMandateContractId { get; private set; }

        /// <summary>
        /// eMandate.ContractSubId as supplied to you by the creditor bank.
        /// If you do not have a ContractSubId, use 0 for this field.
        /// </summary>
        public uint EMandateContractSubId { get; private set; }

        /// <summary>
        /// A valid URL to which the debtor banks redirects to, after the debtor has authorized a transaction.
        /// </summary>
        public string MerchantReturnUrl { get; private set; }

        /// <summary>
        /// An object you can use to load certificates by Fingerprint. By default, it loads from the Windows
        /// certificate store.
        /// </summary>
        public ICertificateLoader CertificateLoader { get; private set; }

        /// <summary>
        /// A string which specifies the fingerprint of the certificate to use to sign messages to the creditor bank.
        /// </summary>
        public string SigningCertificateFingerprint { get; private set; }

        /// <summary>
        /// A string which specifies the fingerprint of the certificate to use to validate messages from the creditor bank.
        /// </summary>
        public string AcquirerCertificateFingerprint { get; private set; }

        /// <summary>
        /// A string which specifies the fingerprint of the second certificate to use to validate messages from the creditor bank.
        /// </summary>
        public string AcquirerAlternateCertificateFingerprint { get; private set; }

        /// <summary>
        /// You may overwrite the signing certificate (which was loaded using SigningCertificateFingerprint), if you want
        /// to load it using a different method.
        /// </summary>
        public X509Certificate2 SigningCertificate { get; private set; }

        /// <summary>
        /// You may overwrite the acquirer certificate (which was loaded using AcquirerCertificateFingerprint), if you want
        /// to load it using a different method.
        /// </summary>
        public X509Certificate2 AcquirerCertificate { get; private set; }

        /// <summary>
        /// You may overwrite the acquirer second certificate (which was loaded using AcquirerCertificateFingerprint), if you want
        /// to load it using a different method.
        /// </summary>
        public X509Certificate2 AcquirerAlternateCertificate { get; private set; }

        /// <summary>
        /// The URL to which the library sends Directory request messages
        /// </summary>
        public string AcquirerUrlDirectoryReq { get; private set; }

        /// <summary>
        /// The URL to which the library sends Transaction request messages (including eMandates messages).
        /// </summary>
        public string AcquirerUrlTransactionReq { get; private set; }

        /// <summary>
        /// The URL to which the library sends Status request messages
        /// </summary>
        public string AcquirerUrlStatusReq { get; private set; }

        /// <summary>
        /// This tells the library that it should save ISO pain raw messages or not. Default is true.
        /// </summary>
        public bool ServiceLogsEnabled { get; private set; }

        /// <summary>
        /// A directory on the disk where the library saves ISO pain raw messages.
        /// </summary>
        public string ServiceLogsLocation { get; private set; }

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
        public string ServiceLogsPattern { get; private set; }

        /// <summary>
        /// The logger factory that creates ILogger objects
        /// </summary>
        public ILoggerFactory LoggerFactory { get; private set; }

        /// <summary>
        /// Ensures that the configuration is valid.
        /// </summary>
        /// <param name="configuration"></param>
        protected static void EnsureIsValid(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            if (string.IsNullOrWhiteSpace(configuration.EMandateContractId))
                throw new ArgumentException("The configuration parameter is not configured.", "EMandateContractId");

            if (string.IsNullOrWhiteSpace(configuration.MerchantReturnUrl))
                throw new ArgumentException("The configuration parameter is not configured.", "MerchantReturnUrl");

            if (string.IsNullOrWhiteSpace(configuration.SigningCertificateFingerprint))
                throw new ArgumentException("The configuration parameter is not configured.", "SigningCertificateFingerprint");
            if (string.IsNullOrWhiteSpace(configuration.AcquirerCertificateFingerprint))
                throw new ArgumentException("The configuration parameter is not configured.", "AcquirerCertificateFingerprint");

            if (string.IsNullOrWhiteSpace(configuration.AcquirerUrlDirectoryReq))
                throw new ArgumentException("The configuration parameter is not configured.", "AcquirerUrlDirectoryReq");
            if (string.IsNullOrWhiteSpace(configuration.AcquirerUrlTransactionReq))
                throw new ArgumentException("The configuration parameter is not configured.", "AcquirerUrlTransactionReq");
            if (string.IsNullOrWhiteSpace(configuration.AcquirerUrlStatusReq))
                throw new ArgumentException("The configuration parameter is not configured.", "AcquirerUrlStatusReq");

            if (configuration.CertificateLoader == null)
                throw new ArgumentException("The configuration parameter is not configured.", "CertificateLoader");
            if (configuration.LoggerFactory == null)
                throw new ArgumentException("The configuration parameter is not configured.", "LoggerFactory");

            if (configuration.SigningCertificate == null)
                throw new CommunicatorException("The signing certificate cannot be loaded.");
            if (configuration.AcquirerCertificate == null)
                throw new CommunicatorException("The acquirer certificate cannot be loaded.");
        }
    }
}
