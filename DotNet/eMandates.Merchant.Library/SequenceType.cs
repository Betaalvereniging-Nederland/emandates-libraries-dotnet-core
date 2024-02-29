
namespace eMandates.Merchant.Library
{
    /// <summary>
    /// Indicates type of eMandate: one-off direct debit or recurring.
    /// </summary>
    public enum SequenceType
    {
        /// <summary>
        /// Recurring mandate
        /// </summary>
        Rcur,

        /// <summary>
        /// One-off mandate
        /// </summary>
        Ooff
    }
}
