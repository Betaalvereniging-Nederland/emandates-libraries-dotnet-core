using eMandates.Merchant.Library;
using eMandates.Merchant.Library.Configuration;

namespace eMandates.Merchant.Website.Models
{
    public class AmendmentResponseViewModel
    {
        public AmendmentResponse Source { get; set; }

        /// <summary>
        /// Maximum amount. Not allowed for Core, optional for B2B.
        /// </summary>
        public Instrumentation Instrumentation { get; set; }
    }
}