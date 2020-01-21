using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eMandates.Merchant.Library
{
    /// <summary>
    /// Describes a status request
    /// </summary>
    public class StatusRequest
    {
        /// <summary>
        /// Parameterless constructor, so it can be used as a Model in views
        /// </summary>
        public StatusRequest()
        {
        }

        /// <summary>
        /// Constructor that highlights all required fields for this object
        /// </summary>
        public StatusRequest(string transactionId)
        {
            this.TransactionId = transactionId;
        }

        /// <summary>
        /// The transaction ID to check
        /// </summary>
        public string TransactionId { get; set; }
    }
}
