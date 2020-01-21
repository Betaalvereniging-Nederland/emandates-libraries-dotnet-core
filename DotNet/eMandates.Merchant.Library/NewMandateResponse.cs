using System;
using eMandates.Merchant.Library.XML.Schemas.iDx;
using eMandates.Merchant.Library.Configuration;

namespace eMandates.Merchant.Library
{
    /// <summary>
    /// Describes a new mandate response
    /// </summary>
    public class NewMandateResponse
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
        /// The URL to which to redirect the creditor so they can authorize the transaction
        /// </summary>
        public string IssuerAuthenticationUrl { get; private set; }

        /// <summary>
        /// The transaction ID
        /// </summary>
        public string TransactionId { get; private set; }

        /// <summary>
        /// DateTime set to when this transaction was created
        /// </summary>
        public DateTime TransactionCreateDateTimestamp { get; private set; }

        /// <summary>
        /// The response XML
        /// </summary>
        public string RawMessage { get; private set; }

        private NewMandateResponse(AcquirerTrxRes trxRes, string xml)
        {
            Error = null;
            IsError = false;
            IssuerAuthenticationUrl = trxRes.Issuer.issuerAuthenticationURL;
            TransactionId = trxRes.Transaction.transactionID;
            TransactionCreateDateTimestamp = trxRes.Transaction.transactionCreateDateTimestamp;
            RawMessage = xml;
        }

        private NewMandateResponse(AcquirerErrorRes errRes, string xml)
        {
            Error = ErrorResponse.Get(errRes);
            IsError = true;
            IssuerAuthenticationUrl = null;
            TransactionId = null;
            TransactionCreateDateTimestamp = default(DateTime);
            RawMessage = xml;
        }

        private NewMandateResponse(Exception e)
        {
            Error = ErrorResponse.Get(e);
            IsError = true;
            IssuerAuthenticationUrl = null;
            TransactionId = null;
            TransactionCreateDateTimestamp = default(DateTime);
        }

        internal static NewMandateResponse Parse(string xml, ILogger logger)
        {
            try
            {
                var dirRes = AcquirerTrxRes.Deserialize(xml);
                return new NewMandateResponse(dirRes, xml);
            }
            catch (Exception e1)
            {
                logger.Log("error : {0}", e1);
                try
                {
                    var errRes = AcquirerErrorRes.Deserialize(xml);
                    return new NewMandateResponse(errRes, xml);
                }
                catch (Exception e2)
                {
                    logger.Log("error : {0}", e2);

                    return new NewMandateResponse(e1)
                    {
                        RawMessage = xml
                    };
                }
            }
        }

        internal static NewMandateResponse Get(Exception e)
        {
            return new NewMandateResponse(e);
        }
    }
}
