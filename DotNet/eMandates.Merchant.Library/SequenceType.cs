using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
