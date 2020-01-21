using eMandates.Merchant.Library.Configuration;

namespace eMandates.Merchant.Website.Models
{
    public class StatusRequestViewModel
    {
        public TransactionReference Source { get; set; }

        /// <summary>
        /// Maximum amount. Not allowed for Core, optional for B2B.
        /// </summary>
        public Instrumentation Instrumentation { get; set; }
    }
}

public class TransactionReference
{
    public string TransactionId { get; set; }
}
