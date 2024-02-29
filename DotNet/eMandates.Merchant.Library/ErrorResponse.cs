using System;
using eMandates.Merchant.Library.XML.Schemas.iDx;

namespace eMandates.Merchant.Library
{
    /// <summary>
    /// Describes an error response
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Unique identification of the error occurring within the iDx transaction
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Descriptive text accompanying Error.errorCode
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Details of the error
        /// </summary>
        public string ErrorDetails { get; set; }

        /// <summary>
        /// Suggestions aimed at resolving the problem
        /// </summary>
        public string SuggestedAction { get; set; }

        /// <summary>
        /// A (standardised) message that the merchant should show to the consumer
        /// </summary>
        public string ConsumerMessage { get; set; }

        private ErrorResponse(AcquirerErrorRes errRes)
        {
            ErrorCode = errRes.Error.errorCode;
            ErrorMessage = errRes.Error.errorMessage;
            ErrorDetails = errRes.Error.errorDetail;
            SuggestedAction = errRes.Error.suggestedAction;
            ConsumerMessage = errRes.Error.consumerMessage;
        }

        private ErrorResponse(Exception e)
        {
            ErrorCode = "";
            ErrorMessage = e.Message;
            ErrorDetails = (e.InnerException != null ? e.InnerException.Message : "");
            SuggestedAction = "";
            ConsumerMessage = "";
        }

        internal static ErrorResponse Get(AcquirerErrorRes errRes)
        {
            return new ErrorResponse(errRes);
        }

        internal static ErrorResponse Get(Exception e)
        {
            return new ErrorResponse(e);
        }
    }
}
