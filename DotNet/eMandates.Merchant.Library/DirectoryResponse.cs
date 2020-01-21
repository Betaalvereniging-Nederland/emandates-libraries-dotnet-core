using System;
using System.Collections.Generic;
using eMandates.Merchant.Library.XML.Schemas.iDx;
using eMandates.Merchant.Library.Configuration;

namespace eMandates.Merchant.Library
{
    /// <summary>
    /// A debtor bank contained in a directory response
    /// </summary>
    public class DebtorBank
    {
        /// <summary>
        /// Country name
        /// </summary>
        public string DebtorBankCountry { get; set; }

        /// <summary>
        /// BIC
        /// </summary>
        public string DebtorBankId { get; set; }

        /// <summary>
        /// Bank name
        /// </summary>
        public string DebtorBankName { get; set; }
    }

    /// <summary>
    /// Describes a directory response
    /// </summary>
    public class DirectoryResponse
    {
        /// <summary>
        /// true if an error occured, or false when no errors were encountered
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Object that holds the error if one occurs; when there are no errors, this is set to null
        /// </summary>
        public ErrorResponse Error { get; private set; }

        /// <summary>
        /// DateTime set to when this directory was last updated
        /// </summary>
        public DateTime DirectoryDateTimestamp { get; private set; }

        /// <summary>
        /// The response XML
        /// </summary>
        public string RawMessage { get; private set; }

        /// <summary>
        /// List of available debtor banks
        /// </summary>
        public List<DebtorBank> DebtorBanks { get; private set; }

        private DirectoryResponse(DirectoryRes dirRes, string xml)
        {
            Error = null;
            IsError = false;
            DirectoryDateTimestamp = dirRes.Directory.directoryDateTimestamp;
            DebtorBanks = new List<DebtorBank>();
            RawMessage = xml;

            foreach (var country in dirRes.Directory.Country)
            {
                foreach (var issuer in country.Issuer)
                {
                    DebtorBanks.Add(new DebtorBank
                    {
                        DebtorBankCountry = country.countryNames,
                        DebtorBankId = issuer.issuerID,
                        DebtorBankName = issuer.issuerName
                    });
                }
            }
        }

        private DirectoryResponse(AcquirerErrorRes errRes, string xml)
        {
            Error = ErrorResponse.Get(errRes);
            IsError = true;
            DirectoryDateTimestamp = default(DateTime);
            DebtorBanks = null;
            RawMessage = xml;
        }

        private DirectoryResponse(Exception e)
        {
            Error = ErrorResponse.Get(e);
            IsError = true;
            DirectoryDateTimestamp = default(DateTime);
            DebtorBanks = null;
        }

        internal static DirectoryResponse Parse(string xml, ILogger logger)
        {
            try
            {
                var dirRes = DirectoryRes.Deserialize(xml);
                return new DirectoryResponse(dirRes, xml);
            }
            catch (Exception e1)
            {
                logger.Log("error : {0}", e1);
                try
                {
                    var errRes = AcquirerErrorRes.Deserialize(xml);
                    return new DirectoryResponse(errRes, xml);
                }
                catch (Exception e2)
                {
                    logger.Log("error : {0}", e2);
                    return new DirectoryResponse(e1)
                    {
                        RawMessage = xml
                    };
                }
            }
        }

        internal static DirectoryResponse Get(Exception e)
        {
            return new DirectoryResponse(e);
        }
    }
}
