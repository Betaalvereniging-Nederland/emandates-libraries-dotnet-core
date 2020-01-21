using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eMandates.Merchant.Library.Misc
{
    /// <summary>
    /// Class that automatically generates MessageId's. You may use this to set the MessageId field manually, or you can use
    /// the constructors for NewMandateRequest, AmendmentRequest or CancellationRequest to do it automatically.
    /// </summary>
    public class MessageIdGenerator
    {
        /// <summary>
        /// Returns a string of 16 alphanumeric characters
        /// </summary>
        public static string New()
        {
            return Guid.NewGuid().GetHashCode().ToString("x") + Guid.NewGuid().GetHashCode().ToString("x");
        }
    }
}
