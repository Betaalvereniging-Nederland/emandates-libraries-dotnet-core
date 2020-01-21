using System;
using eMandates.Merchant.Library.XML.Schemas.iDx;
using eMandates.Merchant.Library.XML.Schemas.pain012;
using eMandates.Merchant.Library.Configuration;

namespace eMandates.Merchant.Library
{
    /// <summary>
    /// Represents a status response
    /// </summary>
    public class StatusResponse
    {
#pragma warning disable 1591
        public static readonly string Open      = "Open";
        public static readonly string Pending   = "Pending";
        public static readonly string Success   = "Success";
        public static readonly string Failure   = "Failure";
        public static readonly string Expired   = "Expired";
        public static readonly string Cancelled = "Cancelled";
#pragma warning restore 1591

        /// <summary>
        /// true if an error occured, or false when no errors were encountered
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Object that holds the error if one occurs; when there are no errors, this is set to null
        /// </summary>
        public ErrorResponse Error { get; private set; }

        /// <summary>
        /// The transaction ID
        /// </summary>
        public string TransactionId { get; private set; }

        /// <summary>
        /// Possible values: Open, Pending, Success, Failure, Expired, Cancelled
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// DateTime when the status was created, or null if no such date available (for example, when mandate has expired)
        /// </summary>
        public DateTime? StatusDateTimestamp { get; private set; }
        
        /// <summary>
        /// The acceptance report returned in the status response
        /// </summary>
        public AcceptanceReport AcceptanceReport { get; private set; }

        /// <summary>
        /// The response XML
        /// </summary>
        public string RawMessage { get; private set; }

        private StatusResponse(AcquirerStatusRes statusRes, string xml)
        {
            Error = null;
            IsError = false;
            TransactionId = statusRes.Transaction.transactionID;
            StatusDateTimestamp =
                statusRes.Transaction.statusDateTimestampSpecified? statusRes.Transaction.statusDateTimestamp : (DateTime?) null;
            Status = statusRes.Transaction.status;
            RawMessage = xml;

            if (Status == StatusResponse.Success)
            {
                if (statusRes.Transaction.container == null)
                    throw new ApplicationException("No mandate present for the transaction with status 'Success'.");

                var mandateXml = statusRes.Transaction.container.Any[0].OuterXml;
                if (!string.IsNullOrWhiteSpace(mandateXml))
                {
                    AcceptanceReport = AcceptanceReport.Parse(mandateXml);
                }
            }
        }

        private StatusResponse(AcquirerErrorRes errRes, string xml)
        {
            Error = ErrorResponse.Get(errRes);
            IsError = true;
            TransactionId = null;
            Status = null;
            StatusDateTimestamp = null;
            RawMessage = xml;
        }

        private StatusResponse(Exception e)
        {
            Error = ErrorResponse.Get(e);
            IsError = true;
            TransactionId = null;
            Status = null;
            StatusDateTimestamp = null;
        }

        internal static StatusResponse Parse(string xml, ILogger logger)
        {            
            try
            {
                var dirRes = AcquirerStatusRes.Deserialize(xml);
                return new StatusResponse(dirRes, xml);
            }
            catch (ApplicationException e)
            {
                return new StatusResponse(e)
                {
                    RawMessage = xml
                };
            }
            catch (Exception e1)
            {
                logger.Log("error : {0}", e1);
                try
                {
                    var errRes = AcquirerErrorRes.Deserialize(xml);
                    return new StatusResponse(errRes, xml);
                }
                catch (Exception e2)
                {
                    logger.Log("error : {0}", e2);
                    return new StatusResponse(e1)
                    {
                        RawMessage = xml
                    };
                }
            }
        }

        internal static StatusResponse Get(Exception e)
        {
            return new StatusResponse(e);
        }
    }
}
