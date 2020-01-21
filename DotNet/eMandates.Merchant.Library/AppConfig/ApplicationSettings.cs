using System;

namespace eMandates.Merchant.Library.AppConfig
{
    public class ApplicationSettings
    {
        public string ContractId { get; set; }
        public uint ContractSubId { get; set; }
        public string MerchantReturnUrl { get; set; } 
        public string AcquirerCertificateThumbprint { get; set; }
        public string AcquirerAlternateCertificateThumbprint { get; set; }
        public string SigningCertificateThumbprint { get; set; }
        public string AcquirerDirectoryRequestUrl { get; set; }
        public string AcquirerTransactionRequestUrl { get; set; }
        public string AcquirerStatusRequestUrl { get; set; }
        public bool ServiceLogsEnabled { get; set; }
        public string ServiceLogsLocation { get; set; }
        public string ServiceLogsPattern { get; set; }
    }
}